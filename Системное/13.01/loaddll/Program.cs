using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace AssemblyLoadContextHost
{
	class Program
	{
		static async Task Main()
		{
			Console.WriteLine("=== РАБОТА С КОНТЕКСТАМИ ЗАГРУЗКИ (AssemblyLoadContext) ===\n");

			// Часть 1: Демонстрация разных контекстов загрузки
			Console.WriteLine("1. ЗАГРУЗКА МОДУЛЕЙ В РАЗНЫХ КОНТЕКСТАХ:");
			await DemonstrateMultipleContexts();

			// Часть 2: Управление жизненным циклом контекстов
			Console.WriteLine("\n2. УПРАВЛЕНИЕ ЖИЗНЕННЫМ ЦИКЛОМ КОНТЕКСТОВ:");
			await DemonstrateContextLifecycle();

			// Часть 3: Изоляция зависимостей
			Console.WriteLine("\n3. ИЗОЛЯЦИЯ ЗАВИСИМОСТЕЙ В КОНТЕКСТАХ:");
			await DemonstrateDependencyIsolation();

			// Часть 4: Проблемы с ссылками и их решение
			Console.WriteLine("\n4. ПРОБЛЕМЫ С ССЫЛКАМИ И ВЫГРУЗКОЙ:");
			await DemonstrateReferenceIssues();

			// Часть 5: Работа с несколькими версиями библиотек
			Console.WriteLine("\n5. НЕСКОЛЬКО ВЕРСИЙ БИБЛИОТЕК В РАЗНЫХ КОНТЕКСТАХ:");
			await DemonstrateMultipleLibraryVersions();
		}

		static async Task DemonstrateMultipleContexts()
		{
			Console.WriteLine($"  ДЕМОНСТРАЦИЯ РАЗНЫХ КОНТЕКСТОВ ЗАГРУЗКИ:");

			// Создаём тестовые конфигурации
			var config1 = new AssemblyLoadContextDemo.ModuleConfiguration
			{
				BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", "DataProcessor"),
				Settings = new Dictionary<string, string>
				{
					["ProcessingMode"] = "Fast",
					["LogLevel"] = "Info"
				}
			};

			var config2 = new AssemblyLoadContextDemo.ModuleConfiguration
			{
				BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", "Analytics"),
				Settings = new Dictionary<string, string>
				{
					["SamplingRate"] = "0.5",
					["MaxMemory"] = "100MB"
				}
			};

			// Получаем путь к текущей сборке (для симуляции загрузки DLL)
			string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
			string currentDirectory = Path.GetDirectoryName(currentAssemblyPath);

			// Симуляция загрузки модулей через ModuleManager
			Console.WriteLine($"\n  Симуляция загрузки модулей...");

			try
			{
				// Создаём модули напрямую (в реальности они загружались бы из DLL)
				// В реальном приложении здесь был бы вызов ModuleManager.LoadModule()

				// Создаём первый изолированный контекст
				Console.WriteLine($"\n  Создаём контекст для DataProcessor:");
				var context1 = new AssemblyLoadContextDemo.ModuleLoadContext("DataProcessor");

				// Загружаем сборку в контекст (симуляция)
				Console.WriteLine($"    Контекст создан: DataProcessor (IsCollectible: {context1.IsCollectible})");

				// Создаём второй изолированный контекст
				Console.WriteLine($"\n  Создаём контекст для Analytics:");
				var context2 = new AssemblyLoadContextDemo.ModuleLoadContext("Analytics");
				Console.WriteLine($"    Контекст создан: Analytics (IsCollectible: {context2.IsCollectible})");

				// Создаём модули в разных контекстах
				var module1 = new AssemblyLoadContextDemo.DataProcessorModule();
				var module2 = new AssemblyLoadContextDemo.AnalyticsModule();

				// Инициализируем модули
				module1.Initialize(config1);
				module2.Initialize(config2);

				// Демонстрация работы модулей
				Console.WriteLine($"\n  ВЫПОЛНЕНИЕ МОДУЛЕЙ В РАЗНЫХ КОНТЕКСТАХ:");

				string[] testInputs = { "Test1", "Test2", "Test3" };

				foreach (string input in testInputs)
				{
					string result1 = module1.Execute(input);
					string result2 = module2.Execute(input);

					Console.WriteLine($"    DataProcessor: {result1}");
					Console.WriteLine($"    Analytics:     {result2}");
				}

				// Проверка статистики
				Console.WriteLine($"\n  СТАТИСТИКА МОДУЛЕЙ:");
				Console.WriteLine($"    DataProcessor обработал: {GetProcessedCount(module1)} элементов");
				Console.WriteLine($"    Analytics метрики: {string.Join(", ", GetMetrics(module2))}");

				// Освобождение ресурсов
				Console.WriteLine($"\n  ОСВОБОЖДЕНИЕ РЕСУРСОВ:");
				module1.Shutdown();
				module2.Shutdown();

				// Очистка контекстов
				context1.PrepareForUnload();
				context2.PrepareForUnload();

				Console.WriteLine($"    Контексты подготовлены к выгрузке");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка: {ex.GetType().Name}: {ex.Message}");
			}
		}

		static async Task DemonstrateContextLifecycle()
		{
			Console.WriteLine($"  УПРАВЛЕНИЕ ЖИЗНЕННЫМ ЦИКЛОМ КОНТЕКСТОВ:");

			// Демонстрация выгрузки контекстов
			Console.WriteLine($"\n  ТЕСТ ВЫГРУЗКИ КОНТЕКСТОВ:");

			WeakReference contextRef = null;
			WeakReference moduleRef = null;

			// Создаём и используем контекст в отдельной области видимости
			{
				Console.WriteLine($"\n  1. Создание контекста и модуля...");
				var config = new AssemblyLoadContextDemo.ModuleConfiguration
				{
					BasePath = "TestPath",
					Settings = new Dictionary<string, string> { ["Test"] = "Value" }
				};

				var localContext = new AssemblyLoadContextDemo.ModuleLoadContext("TemporaryModule");
				var localModule = new AssemblyLoadContextDemo.DataProcessorModule();
				localModule.Initialize(config);

				// Сохраняем слабые ссылки
				contextRef = new WeakReference(localContext);
				moduleRef = new WeakReference(localModule);

				// Используем модуль
				string result = localModule.Execute("Test data");
				Console.WriteLine($"    Результат: {result}");
				Console.WriteLine($"    Обработано: {GetProcessedCount(localModule)} элементов");

				// Явно завершаем работу модуля
				localModule.Shutdown();

				Console.WriteLine($"\n  2. Выход из области видимости...");
				// localContext и localModule выходят из области видимости
			}

			// Сборка мусора
			Console.WriteLine($"\n  3. Принудительная сборка мусора...");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			await Task.Delay(1000); // Даём время на завершение

			// Проверяем, остались ли ссылки
			Console.WriteLine($"\n  4. Проверка ссылок после сборки мусора:");
			bool contextAlive = contextRef?.IsAlive == true;
			bool moduleAlive = moduleRef?.IsAlive == true;

			Console.WriteLine($"    Контекст жив: {contextAlive}");
			Console.WriteLine($"    Модуль жив: {moduleAlive}");

			if (!contextAlive && !moduleAlive)
			{
				Console.WriteLine($"    ✓ Контекст и модуль успешно выгружены!");
			}
			else
			{
				Console.WriteLine($"    ✗ Обнаружены утечки ссылок!");
			}

			// Демонстрация с ModuleManager
			Console.WriteLine($"\n  5. ИСПОЛЬЗОВАНИЕ ModuleManager:");

			// Симуляция загрузки через менеджер
			Console.WriteLine($"    Загрузка модулей через менеджер...");
			// В реальном приложении здесь был бы вызов ModuleManager.LoadModule()

			AssemblyLoadContextDemo.ModuleManager.PrintActiveContexts();
		}

		static async Task DemonstrateDependencyIsolation()
		{
			Console.WriteLine($"  ИЗОЛЯЦИЯ ЗАВИСИМОСТЕЙ В РАЗНЫХ КОНТЕКСТАХ:");

			// Создаём два контекста с разными настройками путей зависимостей
			Console.WriteLine($"\n  Контекст 1: Зависимости из папки 'Deps1'");
			var context1 = new AssemblyLoadContextDemo.ModuleLoadContext("Module1",
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Deps1"));

			Console.WriteLine($"\n  Контекст 2: Зависимости из папки 'Deps2'");
			var context2 = new AssemblyLoadContextDemo.ModuleLoadContext("Module2",
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Deps2"));

			// Демонстрация загрузки одной и той же сборки в разные контексты
			Console.WriteLine($"\n  ДЕМОНСТРАЦИЯ ИЗОЛЯЦИИ:");

			try
			{
				// Симуляция загрузки сборок
				string testAssemblyPath = Assembly.GetExecutingAssembly().Location;

				Console.WriteLine($"\n  Загрузка одной сборки в разные контексты:");
				Console.WriteLine($"    Сборка: {Path.GetFileName(testAssemblyPath)}");

				// В реальном приложении здесь были бы разные версии DLL
				var assembly1 = context1.SafeLoadFromAssemblyPath(testAssemblyPath);
				var assembly2 = context2.SafeLoadFromAssemblyPath(testAssemblyPath);

				if (assembly1 != null && assembly2 != null)
				{
					Console.WriteLine($"    Загружено в контекст 1: {assembly1.GetName().Name}");
					Console.WriteLine($"    Загружено в контекст 2: {assembly2.GetName().Name}");

					// Демонстрация, что это разные экземпляры
					Console.WriteLine($"    Сборки одинаковые: {assembly1 == assembly2}");
					Console.WriteLine($"    Сборки эквивалентны: {Equals(assembly1.GetName(), assembly2.GetName())}");
				}

				// Проверка загрузки системных сборок
				Console.WriteLine($"\n  ПРОВЕРКА СИСТЕМНЫХ СБОРОК:");

				// System.Runtime должна загружаться из контекста по умолчанию
				var systemRuntime1 = context1.LoadFromAssemblyName(
					new AssemblyName("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));

				var systemRuntime2 = context2.LoadFromAssemblyName(
					new AssemblyName("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));

				Console.WriteLine($"    System.Runtime в контексте 1: {systemRuntime1 != null}");
				Console.WriteLine($"    System.Runtime в контексте 2: {systemRuntime2 != null}");

				// Очистка
				context1.PrepareForUnload();
				context2.PrepareForUnload();

				Console.WriteLine($"\n    Контексты подготовлены к выгрузке");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка: {ex.Message}");
			}
		}

		static async Task DemonstrateReferenceIssues()
		{
			Console.WriteLine($"  ПРОБЛЕМЫ С ССЫЛКАМИ И ВЫГРУЗКОЙ КОНТЕКСТОВ:");

			// Сценарий 1: Сохранение сильной ссылки
			Console.WriteLine($"\n  1. ПРОБЛЕМА: СОХРАНЕНИЕ СИЛЬНОЙ ССЫЛКИ");

			AssemblyLoadContextDemo.ModuleLoadContext problematicContext = null;
			WeakReference weakContextRef = null;
			WeakReference weakModuleRef = null;

			// Область видимости для демонстрации проблемы
			{
				var localContext = new AssemblyLoadContextDemo.ModuleLoadContext("Problematic");
				var localModule = new AssemblyLoadContextDemo.DataProcessorModule();
				localModule.Initialize(new AssemblyLoadContextDemo.ModuleConfiguration());

				// ПРОБЛЕМА: сохраняем сильную ссылку вне области видимости
				problematicContext = localContext;

				weakContextRef = new WeakReference(localContext);
				weakModuleRef = new WeakReference(localModule);

				Console.WriteLine($"    Создан контекст и модуль");
				Console.WriteLine($"    Сильная ссылка на контекст сохранена");

				localModule.Execute("Test");
				localModule.Shutdown();
			}

			// Попытка сборки мусора
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Task.Delay(500);

			Console.WriteLine($"\n    После сборки мусора:");
			Console.WriteLine($"    Контекст жив (сильная ссылка): {problematicContext != null}");
			Console.WriteLine($"    Контекст жив (слабая ссылка): {weakContextRef?.IsAlive}");
			Console.WriteLine($"    Модуль жив: {weakModuleRef?.IsAlive}");

			// Очистка сильной ссылки
			problematicContext = null;
			Console.WriteLine($"\n    Сильная ссылка очищена");

			// Ещё одна сборка мусора
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Task.Delay(500);

			Console.WriteLine($"\n    После очистки сильной ссылки:");
			Console.WriteLine($"    Контекст жив: {weakContextRef?.IsAlive}");
			Console.WriteLine($"    Модуль жив: {weakModuleRef?.IsAlive}");

			// Сценарий 2: События и утечки
			Console.WriteLine($"\n  2. ПРОБЛЕМА: СОБЫТИЯ И УТЕЧКИ ПАМЯТИ");

			WeakReference eventContextRef = null;
			WeakReference eventModuleRef = null;

			{
				var eventContext = new AssemblyLoadContextDemo.ModuleLoadContext("EventTest");
				var eventModule = new AssemblyLoadContextDemo.DataProcessorModule();

				// ПОДПИСКА НА СОБЫТИЕ создаёт сильную ссылку
				eventModule.ModuleEvent += (sender, args) =>
					Console.WriteLine($"      Событие: {args}");

				eventContextRef = new WeakReference(eventContext);
				eventModuleRef = new WeakReference(eventModule);

				Console.WriteLine($"    Создан модуль с подпиской на событие");
			}

			// Сборка мусора
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Task.Delay(500);

			Console.WriteLine($"\n    Модуль с подпиской на событие:");
			Console.WriteLine($"    Контекст жив: {eventContextRef?.IsAlive}");
			Console.WriteLine($"    Модуль жив: {eventModuleRef?.IsAlive}");

			if (eventModuleRef?.IsAlive == true)
			{
				Console.WriteLine($"    ✓ Доказана проблема: событие держит ссылку на модуль");
			}
		}

		static async Task DemonstrateMultipleLibraryVersions()
		{
			Console.WriteLine($"  НЕСКОЛЬКО ВЕРСИЙ БИБЛИОТЕК В РАЗНЫХ КОНТЕКСТАХ:");

			Console.WriteLine($"\n  СЦЕНАРИЙ: Модули с разными версиями библиотек");

			// Конфигурации для модулей
			var legacyConfig = new AssemblyLoadContextDemo.ModuleConfiguration
			{
				BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Legacy"),
				Settings = new Dictionary<string, string> { ["CompatibilityMode"] = "Legacy" }
			};

			var modernConfig = new AssemblyLoadContextDemo.ModuleConfiguration
			{
				BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modern"),
				Settings = new Dictionary<string, string> { ["CompatibilityMode"] = "Modern" }
			};

			try
			{
				// Создаём контексты для разных версий
				Console.WriteLine($"\n  1. Создание контекстов:");
				var legacyContext = new AssemblyLoadContextDemo.ModuleLoadContext("LegacyModule");
				var modernContext = new AssemblyLoadContextDemo.ModuleLoadContext("ModernModule");

				// Создаём модули разных поколений
				Console.WriteLine($"\n  2. Создание модулей разных версий:");
				var legacyModule = new AssemblyLoadContextDemo.LegacyModule();
				var modernModule = new AssemblyLoadContextDemo.DataProcessorModule();

				// Инициализация
				legacyModule.Initialize(legacyConfig);
				modernModule.Initialize(modernConfig);

				// Демонстрация работы
				Console.WriteLine($"\n  3. ВЫПОЛНЕНИЕ В РАЗНЫХ КОНТЕКСТАХ:");

				string testData = "SharedTestData";

				string legacyResult = legacyModule.Execute(testData);
				string modernResult = modernModule.Execute(testData);

				Console.WriteLine($"    LegacyModule: {legacyResult}");
				Console.WriteLine($"    ModernModule: {modernResult}");

				// Демонстрация различий в API
				Console.WriteLine($"\n  4. РАЗЛИЧИЯ В API:");

				Type legacyType = legacyModule.GetType();
				Type modernType = modernModule.GetType();

				Console.WriteLine($"    LegacyModule методы: {string.Join(", ", legacyType.GetMethods().Select(m => m.Name).Take(5))}");
				Console.WriteLine($"    ModernModule методы: {string.Join(", ", modernType.GetMethods().Select(m => m.Name).Take(5))}");

				// Проверка устаревших методов
				var obsoleteMethod = legacyType.GetMethod("ProcessLegacy");
				if (obsoleteMethod != null)
				{
					var obsoleteAttr = obsoleteMethod.GetCustomAttribute<ObsoleteAttribute>();
					if (obsoleteAttr != null)
					{
						Console.WriteLine($"    LegacyModule.ProcessLegacy устарел: {obsoleteAttr.Message}");
					}
				}

				// Очистка
				Console.WriteLine($"\n  5. ОЧИСТКА КОНТЕКСТОВ:");
				legacyModule.Shutdown();
				modernModule.Shutdown();

				legacyContext.PrepareForUnload();
				modernContext.PrepareForUnload();

				Console.WriteLine($"    Контексты подготовлены к выгрузке");

				// Проверка возможности горячей замены
				Console.WriteLine($"\n  6. ДЕМОНСТРАЦИЯ ГОРЯЧЕЙ ЗАМЕНЫ:");

				// Симуляция: выгружаем старый, загружаем новый
				Console.WriteLine($"    Выгрузка LegacyModule...");
				// В реальности: legacyContext.Unload();

				Console.WriteLine($"    Загрузка новой версии...");
				var newContext = new AssemblyLoadContextDemo.ModuleLoadContext("UpdatedModule");
				var updatedModule = new AssemblyLoadContextDemo.DataProcessorModule();
				updatedModule.Initialize(modernConfig);

				string updatedResult = updatedModule.Execute("New data");
				Console.WriteLine($"    Обновлённый модуль: {updatedResult}");

				updatedModule.Shutdown();
				newContext.PrepareForUnload();

			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка: {ex.GetType().Name}: {ex.Message}");
			}
		}

		// Вспомогательные методы для доступа к защищённым данным
		static int GetProcessedCount(AssemblyLoadContextDemo.DataProcessorModule module)
		{
			try
			{
				var method = module.GetType().GetMethod("GetProcessedCount",
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.Instance);

				return method != null ? (int)method.Invoke(module, null) : 0;
			}
			catch
			{
				return 0;
			}
		}

		static string GetMetrics(AssemblyLoadContextDemo.AnalyticsModule module)
		{
			try
			{
				var method = module.GetType().GetMethod("GetMetrics",
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.Instance);

				if (method != null)
				{
					var metrics = (Dictionary<string, int>)method.Invoke(module, null);
					return string.Join(", ", metrics.Select(kv => $"{kv.Key}: {kv.Value}"));
				}
			}
			catch { }

			return "N/A";
		}
	}
}