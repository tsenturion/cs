using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System.Linq;

namespace AssemblyLoadContextDemo
{
	// Общие контракты, которые будут загружены в основной контекст
	public interface ILoadableModule
	{
		string ModuleName { get; }
		string Version { get; }
		void Initialize(ModuleConfiguration config);
		string Execute(string input);
		void Shutdown();
	}

	public interface IConfigurable
	{
		Dictionary<string, object> GetConfiguration();
		void UpdateConfiguration(Dictionary<string, object> config);
	}

	// Класс для передачи конфигурации между контекстами
	[Serializable]
	public class ModuleConfiguration
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string BasePath { get; set; }
		public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

		public override string ToString() => $"Config[{Id}]: {Settings.Count} settings";
	}

	// Результат выполнения модуля, передаваемый между контекстами
	[Serializable]
	public class ModuleResult
	{
		public bool Success { get; set; }
		public string Output { get; set; }
		public TimeSpan ExecutionTime { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.Now;

		public override string ToString() =>
			$"[{Timestamp:HH:mm:ss}] {(Success ? "✓" : "✗")} {Output} ({ExecutionTime.TotalMilliseconds:F0}ms)";
	}

	// Пользовательский контекст загрузки для изоляции модулей
	public class ModuleLoadContext : AssemblyLoadContext
	{
		private readonly string _moduleName;
		private readonly string _dependenciesPath;
		private readonly List<Assembly> _loadedAssemblies = new List<Assembly>();

		public ModuleLoadContext(string moduleName, string dependenciesPath = null)
			: base(isCollectible: true) // isCollectible: true позволяет выгрузку
		{
			_moduleName = moduleName;
			_dependenciesPath = dependenciesPath;
			Console.WriteLine($"[ModuleLoadContext] Создан контекст для модуля: {moduleName}");
		}

		// Переопределение метода загрузки зависимостей
		protected override Assembly Load(AssemblyName assemblyName)
		{
			Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Запрос на загрузку: {assemblyName.Name}");

			// Сначала пробуем загрузить из кастомной папки зависимостей
			if (!string.IsNullOrEmpty(_dependenciesPath))
			{
				string assemblyPath = Path.Combine(_dependenciesPath, $"{assemblyName.Name}.dll");
				if (File.Exists(assemblyPath))
				{
					Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Загружаем из {assemblyPath}");
					Assembly assembly = LoadFromAssemblyPath(assemblyPath);
					_loadedAssemblies.Add(assembly);
					return assembly;
				}
			}

			// Для системных сборок используем контекст по умолчанию
			if (assemblyName.Name.StartsWith("System") ||
				assemblyName.Name == "netstandard" ||
				assemblyName.Name == "mscorlib")
			{
				Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Используем системную сборку: {assemblyName.Name}");
				return null; // null означает загрузку из контекста по умолчанию
			}

			// Ищем в папке модуля
			string moduleAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
													_moduleName,
													$"{assemblyName.Name}.dll");
			if (File.Exists(moduleAssemblyPath))
			{
				Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Загружаем из модуля: {assemblyName.Name}");
				Assembly assembly = LoadFromAssemblyPath(moduleAssemblyPath);
				_loadedAssemblies.Add(assembly);
				return assembly;
			}

			// Если не нашли - используем контекст по умолчанию
			Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Используем контекст по умолчанию для: {assemblyName.Name}");
			return null;
		}

		// Метод для безопасной загрузки сборки
		public Assembly SafeLoadFromAssemblyPath(string assemblyPath)
		{
			try
			{
				Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Загружаем: {Path.GetFileName(assemblyPath)}");
				Assembly assembly = LoadFromAssemblyPath(assemblyPath);
				_loadedAssemblies.Add(assembly);
				return assembly;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Ошибка загрузки: {ex.Message}");
				return null;
			}
		}

		// Информация о загруженных сборках
		public void PrintLoadedAssemblies()
		{
			Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Загруженные сборки:");
			foreach (var assembly in _loadedAssemblies)
			{
				Console.WriteLine($"  - {assembly.GetName().Name} v{assembly.GetName().Version}");
			}
		}

		// Очистка перед выгрузкой
		public void PrepareForUnload()
		{
			Console.WriteLine($"[ModuleLoadContext:{_moduleName}] Подготовка к выгрузке...");
			_loadedAssemblies.Clear();
		}
	}

	// Базовый класс для модулей в изолированных контекстах
	public abstract class BaseModule : ILoadableModule, IDisposable
	{
		public abstract string ModuleName { get; }
		public abstract string Version { get; }

		protected ModuleConfiguration Configuration { get; private set; }
		protected bool IsInitialized { get; private set; }
		protected bool IsDisposed { get; private set; }

		// Событие для уведомления хоста
		public event EventHandler<string> ModuleEvent;

		protected virtual void OnModuleEvent(string message)
		{
			ModuleEvent?.Invoke(this, message);
		}

		public virtual void Initialize(ModuleConfiguration config)
		{
			if (IsDisposed)
				throw new ObjectDisposedException(ModuleName);

			Configuration = config;
			IsInitialized = true;
			OnModuleEvent($"Модуль {ModuleName} v{Version} инициализирован");
		}

		public abstract string Execute(string input);

		public virtual void Shutdown()
		{
			OnModuleEvent($"Модуль {ModuleName} завершает работу");
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// Освобождение управляемых ресурсов
					OnModuleEvent($"Модуль {ModuleName} освобождает ресурсы");
				}

				IsDisposed = true;
				IsInitialized = false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~BaseModule()
		{
			Dispose(false);
		}
	}

	// Пример модуля, который должен быть загружен в изолированном контексте
	public class DataProcessorModule : BaseModule, IConfigurable
	{
		public override string ModuleName => "Data Processor";
		public override string Version => "2.0.0";

		private List<string> _processedItems = new List<string>();
		private Dictionary<string, object> _config = new Dictionary<string, object>
		{
			["MaxItems"] = 100,
			["EnableLogging"] = true,
			["ProcessingMode"] = "Standard"
		};

		public override void Initialize(ModuleConfiguration config)
		{
			base.Initialize(config);

			if (config != null && config.Settings != null)
			{
				foreach (var setting in config.Settings)
				{
					_config[setting.Key] = setting.Value;
				}
			}

			Console.WriteLine($"[{ModuleName}] Инициализация в изолированном контексте");
			Console.WriteLine($"[{ModuleName}] Базовая папка: {config?.BasePath ?? "N/A"}");
		}

		public override string Execute(string input)
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Модуль не инициализирован");

			string result = $"Обработано: {input} (режим: {_config["ProcessingMode"]})";
			_processedItems.Add(result);

			// Симуляция нагрузки
			System.Threading.Thread.Sleep(50);

			return result;
		}

		public Dictionary<string, object> GetConfiguration()
		{
			return new Dictionary<string, object>(_config);
		}

		public void UpdateConfiguration(Dictionary<string, object> config)
		{
			foreach (var item in config)
			{
				_config[item.Key] = item.Value;
			}
			OnModuleEvent($"Конфигурация обновлена: {string.Join(", ", config.Keys)}");
		}

		public int GetProcessedCount() => _processedItems.Count;

		public string GetStatistics()
		{
			return $"Обработано элементов: {_processedItems.Count}, " +
				   $"Память: {GC.GetTotalMemory(false) / 1024} KB";
		}
	}

	// Другой модуль с разными зависимостями
	public class AnalyticsModule : BaseModule
	{
		public override string ModuleName => "Analytics Engine";
		public override string Version => "1.5.0";

		private Random _random = new Random();
		private Dictionary<string, int> _metrics = new Dictionary<string, int>();

		public override void Initialize(ModuleConfiguration config)
		{
			base.Initialize(config);
			Console.WriteLine($"[{ModuleName}] Запуск аналитического движка");

			// Инициализация метрик
			_metrics["Processed"] = 0;
			_metrics["Errors"] = 0;
			_metrics["Warnings"] = 0;
		}

		public override string Execute(string input)
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Модуль не инициализирован");

			_metrics["Processed"]++;

			// Симуляция анализа
			int score = AnalyzeData(input);
			string result = $"Анализ: '{input}' → score: {score}/100";

			if (score < 50)
				_metrics["Warnings"]++;

			return result;
		}

		private int AnalyzeData(string data)
		{
			// Упрощённый "анализ"
			int lengthScore = Math.Min(data.Length * 2, 50);
			int complexityScore = _random.Next(0, 50);
			return lengthScore + complexityScore;
		}

		public Dictionary<string, int> GetMetrics()
		{
			return new Dictionary<string, int>(_metrics);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Console.WriteLine($"[{ModuleName}] Сохранение метрик...");
				// Здесь могло бы быть сохранение в файл/БД
			}
			base.Dispose(disposing);
		}
	}

	// Модуль, который симулирует использование разных версий библиотек
	public class LegacyModule : BaseModule
	{
		public override string ModuleName => "Legacy Support";
		public override string Version => "1.0.0";

		private List<string> _legacyData = new List<string>();

		public override void Initialize(ModuleConfiguration config)
		{
			base.Initialize(config);
			Console.WriteLine($"[{ModuleName}] Запуск legacy-модуля (старая версия API)");

			// Загрузка legacy данных
			_legacyData.Add("LegacyItem1");
			_legacyData.Add("LegacyItem2");
		}

		public override string Execute(string input)
		{
			_legacyData.Add(input);

			// Старый формат вывода
			return $"LEGACY_FORMAT:{input}:{DateTime.Now:yyyyMMdd}:{_legacyData.Count}";
		}

		public string[] GetLegacyData()
		{
			return _legacyData.ToArray();
		}

		// Устаревший метод (для демонстрации)
		[Obsolete("Используйте Execute вместо этого метода")]
		public string ProcessLegacy(string input)
		{
			return $"OLD: {input}";
		}
	}

	// Класс для управления контекстами загрузки
	public static class ModuleManager
	{
		private static Dictionary<string, (ModuleLoadContext context, WeakReference reference)>
			_activeContexts = new Dictionary<string, (ModuleLoadContext, WeakReference)>();

		public static ILoadableModule LoadModule(string moduleName, string assemblyPath, ModuleConfiguration config)
		{
			Console.WriteLine($"\n[MODULE MANAGER] Загрузка модуля: {moduleName}");
			Console.WriteLine($"  Assembly: {Path.GetFileName(assemblyPath)}");
			Console.WriteLine($"  Context: Изолированный");

			// Создаём новый контекст для модуля
			var moduleContext = new ModuleLoadContext(moduleName);

			// Загружаем сборку в изолированный контекст
			Assembly moduleAssembly = moduleContext.SafeLoadFromAssemblyPath(assemblyPath);
			if (moduleAssembly == null)
			{
				throw new FileNotFoundException($"Не удалось загрузить сборку: {assemblyPath}");
			}

			// Ищем тип модуля
			Type moduleType = moduleAssembly.GetTypes()
				.FirstOrDefault(t => typeof(ILoadableModule).IsAssignableFrom(t) &&
									!t.IsInterface && !t.IsAbstract);

			if (moduleType == null)
			{
				throw new TypeLoadException($"В сборке не найден тип, реализующий ILoadableModule");
			}

			// Создаём экземпляр модуля
			ILoadableModule module = (ILoadableModule)Activator.CreateInstance(moduleType);

			// Инициализируем модуль
			module.Initialize(config);

			// Сохраняем контекст со слабой ссылкой
			_activeContexts[moduleName] = (moduleContext, new WeakReference(module));

			Console.WriteLine($"  [MODULE MANAGER] Модуль загружен в изолированный контекст");
			moduleContext.PrintLoadedAssemblies();

			return module;
		}

		public static bool TryUnloadModule(string moduleName)
		{
			Console.WriteLine($"\n[MODULE MANAGER] Попытка выгрузки модуля: {moduleName}");

			if (!_activeContexts.TryGetValue(moduleName, out var contextInfo))
			{
				Console.WriteLine($"  Модуль не найден: {moduleName}");
				return false;
			}

			var (context, weakRef) = contextInfo;

			// Проверяем, есть ли активные ссылки на модуль
			if (weakRef.IsAlive)
			{
				Console.WriteLine($"  Модуль всё ещё имеет активные ссылки, выгрузка невозможна");
				return false;
			}

			try
			{
				// Подготавливаем контекст к выгрузке
				context.PrepareForUnload();

				// Инициируем выгрузку
				context.Unload();

				// Удаляем из словаря
				_activeContexts.Remove(moduleName);

				Console.WriteLine($"  Контекст модуля {moduleName} выгружен");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка при выгрузке: {ex.Message}");
				return false;
			}
		}

		public static void PrintActiveContexts()
		{
			Console.WriteLine($"\n[MODULE MANAGER] Активные контексты загрузки:");

			if (_activeContexts.Count == 0)
			{
				Console.WriteLine("  Нет активных контекстов");
				return;
			}

			foreach (var kvp in _activeContexts)
			{
				bool isAlive = kvp.Value.reference.IsAlive;
				Console.WriteLine($"  - {kvp.Key}: ссылки активны = {isAlive}");
			}
		}
	}
}