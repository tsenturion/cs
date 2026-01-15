using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PluginHostApplication
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("=== ДИНАМИЧЕСКАЯ ЗАГРУЗКА DLL ВО ВРЕМЯ ВЫПОЛНЕНИЯ ===\n");

			// Часть 1: Загрузка и обнаружение плагинов
			Console.WriteLine("1. ЗАГРУЗКА И ОБНАРУЖЕНИЕ ПЛАГИНОВ:");
			LoadAndDiscoverPlugins();

			// Часть 2: Работа с контрактами
			Console.WriteLine("\n2. РАБОТА ЧЕРЕЗ КОНТРАКТЫ:");
			WorkThroughContracts();

			// Часть 3: Обработка зависимостей и версий
			Console.WriteLine("\n3. УПРАВЛЕНИЕ ЗАВИСИМОСТЯМИ И ВЕРСИЯМИ:");
			ManageDependenciesAndVersions();

			// Часть 4: Изоляция и обработка ошибок
			Console.WriteLine("\n4. ИЗОЛЯЦИЯ И ОБРАБОТКА ОШИБОК:");
			DemonstrateErrorIsolation();

			// Часть 5: Динамическая замена плагинов
			Console.WriteLine("\n5. ДИНАМИЧЕСКАЯ ЗАМЕНА ПЛАГИНОВ:");
			DemonstrateDynamicReplacement();
		}

		static void LoadAndDiscoverPlugins()
		{
			string pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

			Console.WriteLine($"  Поиск плагинов в: {pluginsDirectory}");

			if (!Directory.Exists(pluginsDirectory))
			{
				Directory.CreateDirectory(pluginsDirectory);
				Console.WriteLine($"  Создана директория для плагинов");
				Console.WriteLine($"  Скопируйте скомпилированные DLL плагинов в эту папку");
				return;
			}

			string[] dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");
			Console.WriteLine($"  Найдено DLL файлов: {dllFiles.Length}");

			List<Assembly> loadedAssemblies = new List<Assembly>();
			List<Type> pluginTypes = new List<Type>();

			foreach (string dllPath in dllFiles)
			{
				try
				{
					Console.WriteLine($"\n  Загрузка: {Path.GetFileName(dllPath)}");

					// Загрузка сборки в домен приложения
					Assembly assembly = Assembly.LoadFrom(dllPath);
					loadedAssemblies.Add(assembly);

					Console.WriteLine($"    Сборка загружена: {assembly.GetName().Name}");
					Console.WriteLine($"    Версия: {assembly.GetName().Version}");
					Console.WriteLine($"    Расположение: {Path.GetFileName(assembly.Location)}");

					// Поиск типов, реализующих IPlugin
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						// Проверяем, реализует ли тип интерфейс IPlugin
						if (typeof(DynamicLoadingDemo.IPlugin).IsAssignableFrom(type) &&
							!type.IsInterface && !type.IsAbstract)
						{
							pluginTypes.Add(type);
							Console.WriteLine($"    Найден плагин: {type.Name}");
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Console.WriteLine($"    Ошибка загрузки типов: {ex.Message}");
					Console.WriteLine($"    LoaderExceptions: {string.Join(", ", ex.LoaderExceptions.Select(e => e.Message))}");
				}
				catch (FileLoadException ex)
				{
					Console.WriteLine($"    Ошибка загрузки файла: {ex.Message}");
					Console.WriteLine($"    FusionLog: {ex.FusionLog}");
				}
				catch (BadImageFormatException ex)
				{
					Console.WriteLine($"    Некорректный формат DLL: {ex.Message}");
					Console.WriteLine($"    Возможно, DLL скомпилирована для другой версии .NET");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    Общая ошибка: {ex.GetType().Name}: {ex.Message}");
				}
			}

			Console.WriteLine($"\n  ИТОГО: Загружено сборок: {loadedAssemblies.Count}, Найдено плагинов: {pluginTypes.Count}");

			// Создание и инициализация плагинов
			if (pluginTypes.Count > 0)
			{
				Console.WriteLine($"\n  ИНИЦИАЛИЗАЦИЯ ПЛАГИНОВ:");
				List<DynamicLoadingDemo.IPlugin> initializedPlugins = new List<DynamicLoadingDemo.IPlugin>();

				foreach (Type pluginType in pluginTypes)
				{
					try
					{
						Console.Write($"    {pluginType.Name}: ");

						// Создание экземпляра плагина
						DynamicLoadingDemo.IPlugin plugin = (DynamicLoadingDemo.IPlugin)Activator.CreateInstance(pluginType);

						// Инициализация
						plugin.Initialize();

						initializedPlugins.Add(plugin);
						Console.WriteLine($"успешно");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"ошибка: {ex.Message}");
					}
				}

				Console.WriteLine($"\n  Инициализировано плагинов: {initializedPlugins.Count}/{pluginTypes.Count}");
			}
		}

		static void WorkThroughContracts()
		{
			Console.WriteLine($"  РАБОТА ЧЕРЕЗ ИНТЕРФЕЙСЫ (КОНТРАКТЫ):");

			// Симуляция загрузки плагинов
			var plugins = new List<DynamicLoadingDemo.IPlugin>
			{
				new DynamicLoadingDemo.TextProcessorPlugin(),
				new DynamicLoadingDemo.AdvancedDataPlugin(),
				new DynamicLoadingDemo.MathPlugin()
			};

			// Инициализация всех плагинов
			foreach (var plugin in plugins)
			{
				try
				{
					plugin.Initialize();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    Ошибка инициализации {plugin.Name}: {ex.Message}");
				}
			}

			// Работа через контракт IPlugin
			Console.WriteLine($"\n  ОБРАБОТКА ДАННЫХ ЧЕРЕЗ IPlugin:");
			string testData = "Пример данных для обработки";

			foreach (var plugin in plugins)
			{
				try
				{
					string result = plugin.Process(testData);
					Console.WriteLine($"    {plugin.Name}: {result}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    {plugin.Name}: ошибка - {ex.Message}");
				}
			}

			// Работа через дополнительный интерфейс IDataProcessor
			Console.WriteLine($"\n  ОБРАБОТКА ЧЕРЕЗ ДОПОЛНИТЕЛЬНЫЕ ИНТЕРФЕЙСЫ:");

			var complexData = new DynamicLoadingDemo.ComplexData(
				("Name", "Test Object"),
				("Value", 123),
				("Timestamp", DateTime.Now)
			);

			foreach (var plugin in plugins)
			{
				if (plugin is DynamicLoadingDemo.IDataProcessor dataProcessor)
				{
					string result = dataProcessor.ProcessData(complexData);
					Console.WriteLine($"    {plugin.Name} как IDataProcessor: {result}");
				}
			}

			// Вызов специфичных методов (если доступны через конкретный тип)
			Console.WriteLine($"\n  ДОСТУП К СПЕЦИФИЧНЫМ МЕТОДАМ:");

			foreach (var plugin in plugins)
			{
				if (plugin is DynamicLoadingDemo.AdvancedDataPlugin advancedPlugin)
				{
					string analysis = advancedPlugin.Analyze(testData);
					Console.WriteLine($"    AdvancedDataPlugin.Analyze(): {analysis}");
				}
				else if (plugin is DynamicLoadingDemo.MathPlugin mathPlugin)
				{
					double expression = mathPlugin.CalculateExpression(testData);
					Console.WriteLine($"    MathPlugin.CalculateExpression(): {expression:F2}");
				}
				else if (plugin is DynamicLoadingDemo.TextProcessorPlugin textPlugin)
				{
					// Получаем специфичный метод через reflection
					Type type = plugin.GetType();
					MethodInfo countMethod = type.GetMethod("GetProcessedCount");

					if (countMethod != null)
					{
						int count = (int)countMethod.Invoke(plugin, null);
						Console.WriteLine($"    TextProcessorPlugin.GetProcessedCount(): {count}");
					}
				}
			}
		}

		static void ManageDependenciesAndVersions()
		{
			Console.WriteLine($"  УПРАВЛЕНИЕ ВЕРСИЯМИ И ЗАВИСИМОСТЯМИ:");

			// Создание плагинов с разными версиями
			var plugins = new DynamicLoadingDemo.IPlugin[]
			{
				new DynamicLoadingDemo.TextProcessorPlugin(),      // v1.0.0
                new DynamicLoadingDemo.AdvancedDataPlugin(),       // v2.0.0
                new DynamicLoadingDemo.MathPlugin()                // v1.5.0
            };

			string hostVersion = "1.1.0";
			Console.WriteLine($"    Версия хоста: {hostVersion}");

			Console.WriteLine($"\n    ПРОВЕРКА СОВМЕСТИМОСТИ ВЕРСИЙ:");
			foreach (var plugin in plugins)
			{
				bool isCompatible = DynamicLoadingDemo.PluginHostUtilities.ValidatePluginVersion(
					plugin.Version, hostVersion);

				string status = isCompatible ? "совместим" : "не совместим";
				Console.WriteLine($"      {plugin.Name} v{plugin.Version}: {status}");
			}

			Console.WriteLine($"\n    ЧТЕНИЕ МЕТАДАННЫХ ПЛАГИНОВ:");
			foreach (var plugin in plugins)
			{
				Type pluginType = plugin.GetType();
				var metadataAttr = (DynamicLoadingDemo.PluginMetadataAttribute)
					Attribute.GetCustomAttribute(pluginType, typeof(DynamicLoadingDemo.PluginMetadataAttribute));

				if (metadataAttr != null)
				{
					Console.WriteLine($"      {plugin.Name}:");
					Console.WriteLine($"        Автор: {metadataAttr.Author}");
					Console.WriteLine($"        Описание: {metadataAttr.Description}");
					Console.WriteLine($"        Требуемая версия хоста: {metadataAttr.RequiredHostVersion}");
				}
			}

			Console.WriteLine($"\n    ПРОВЕРКА ЗАВИСИМОСТЕЙ:");
			// Симуляция проверки зависимостей сборки
			try
			{
				Assembly mathAssembly = Assembly.GetAssembly(typeof(DynamicLoadingDemo.MathPlugin));
				AssemblyName[] references = mathAssembly.GetReferencedAssemblies();

				Console.WriteLine($"      MathPlugin зависимости:");
				foreach (var reference in references.Take(3)) // Показываем только первые 3
				{
					Console.WriteLine($"        - {reference.Name} v{reference.Version}");
				}
				if (references.Length > 3)
					Console.WriteLine($"        ... и ещё {references.Length - 3} зависимостей");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"      Ошибка при проверке зависимостей: {ex.Message}");
			}
		}

		static void DemonstrateErrorIsolation()
		{
			Console.WriteLine($"  ИЗОЛЯЦИЯ ОШИБОК ПЛАГИНОВ:");

			var plugins = new DynamicLoadingDemo.IPlugin[]
			{
				new DynamicLoadingDemo.TextProcessorPlugin(),
				new DynamicLoadingDemo.BuggyPlugin(),  // Плагин с ошибками
                new DynamicLoadingDemo.MathPlugin()
			};

			// Инициализация с изоляцией ошибок
			Console.WriteLine($"\n    ИНИЦИАЛИЗАЦИЯ С ОБРАБОТКОЙ ОШИБОК:");
			int successful = 0;

			foreach (var plugin in plugins)
			{
				try
				{
					plugin.Initialize();
					successful++;
					Console.WriteLine($"      {plugin.Name}: успешно");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"      {plugin.Name}: ОШИБКА - {ex.Message}");
					// Ошибка одного плагина не должна влиять на другие
				}
			}

			Console.WriteLine($"      Инициализировано: {successful}/{plugins.Length}");

			// Обработка данных с изоляцией ошибок
			Console.WriteLine($"\n    ОБРАБОТКА ДАННЫХ С ОБРАБОТКОЙ ОШИБОК:");
			string[] testInputs = { "test", "crash", "123", "" };

			foreach (var plugin in plugins.Where(p => p != null))
			{
				Console.WriteLine($"\n      {plugin.Name}:");

				foreach (string input in testInputs)
				{
					try
					{
						string result = plugin.Process(input);
						Console.WriteLine($"        '{input}' -> {result}");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"        '{input}' -> ОШИБКА: {ex.GetType().Name}: {ex.Message}");
						// Продолжаем обработку следующих входных данных
					}
				}
			}
		}

		static void DemonstrateDynamicReplacement()
		{
			Console.WriteLine($"  ДИНАМИЧЕСКАЯ ЗАМЕНА ПЛАГИНОВ:");

			// Симуляция загрузки плагина из "новой версии"
			Console.WriteLine($"\n    СИМУЛЯЦИЯ ОБНОВЛЕНИЯ ПЛАГИНА:");

			try
			{
				// Создаем "старый" плагин
				DynamicLoadingDemo.IPlugin oldPlugin = new DynamicLoadingDemo.TextProcessorPlugin();
				oldPlugin.Initialize();

				Console.WriteLine($"      Старый плагин: {oldPlugin.Name} v{oldPlugin.Version}");
				string oldResult = oldPlugin.Process("test");
				Console.WriteLine($"      Результат старого: {oldResult}");

				// Симуляция загрузки "нового" плагина
				Console.WriteLine($"\n      Загрузка новой версии...");

				// В реальном приложении здесь была бы загрузка из новой DLL
				// Для демонстрации создаем другой плагин
				DynamicLoadingDemo.IPlugin newPlugin = new DynamicLoadingDemo.AdvancedDataPlugin();
				newPlugin.Initialize();

				Console.WriteLine($"      Новый плагин: {newPlugin.Name} v{newPlugin.Version}");
				string newResult = newPlugin.Process("test");
				Console.WriteLine($"      Результат нового: {newResult}");

				// Демонстрация горячей замены
				Console.WriteLine($"\n      ГОРЯЧАЯ ЗАМЕНА ПЛАГИНОВ:");

				// Создаем список активных плагинов
				var activePlugins = new List<DynamicLoadingDemo.IPlugin> { oldPlugin };
				Console.WriteLine($"      Активные плагины до замены: {string.Join(", ", activePlugins.Select(p => p.Name))}");

				// Заменяем плагин
				activePlugins.Remove(oldPlugin);
				activePlugins.Add(newPlugin);

				Console.WriteLine($"      Активные плагины после замены: {string.Join(", ", activePlugins.Select(p => p.Name))}");

				// Проверка работы после замены
				Console.WriteLine($"\n      ПРОВЕРКА РАБОТЫ ПОСЛЕ ЗАМЕНЫ:");
				foreach (var plugin in activePlugins)
				{
					string result = plugin.Process("новые данные");
					Console.WriteLine($"        {plugin.Name}: {result}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"      Ошибка при динамической замене: {ex.Message}");
			}
		}
	}
}