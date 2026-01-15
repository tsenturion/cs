using System;
using System.Reflection;
using System.Collections.Generic;
using ReflectionDemoLibrary;

namespace ReflectionConsumer
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("=== ДЕМОНСТРАЦИЯ REFLECTION ===\n");

			// Часть 1: Обычное использование через статическую типизацию
			Console.WriteLine("1. СТАТИЧЕСКОЕ ИСПОЛЬЗОВАНИЕ ТИПОВ:");
			DemonstrateStaticUsage();

			// Часть 2: Использование Reflection для исследования
			Console.WriteLine("\n2. ИССЛЕДОВАНИЕ ТИПОВ ЧЕРЕЗ REFLECTION:");
			DemonstrateReflectionExploration();

			// Часть 3: Динамическое создание и вызов
			Console.WriteLine("\n3. ДИНАМИЧЕСКОЕ СОЗДАНИЕ И ВЫЗОВ:");
			DemonstrateDynamicCreation();

			// Часть 4: Работа с атрибутами
			Console.WriteLine("\n4. РАБОТА С АТРИБУТАМИ:");
			DemonstrateAttributeReading();

			// Часть 5: Сравнение производительности
			Console.WriteLine("\n5. СРАВНЕНИЕ ПРОИЗВОДИТЕЛЬНОСТИ:");
			ComparePerformance();
		}

		static void DemonstrateStaticUsage()
		{
			// Компилятор знает типы, IntelliSense работает
			IPlugin simplePlugin = new SimplePlugin();
			IPlugin advancedPlugin = new AdvancedPlugin();

			Console.WriteLine($"  Простое использование:");
			Console.WriteLine($"    simplePlugin.Name: {simplePlugin.Name}");
			Console.WriteLine($"    simplePlugin.GetInfo(): {simplePlugin.GetInfo()}");

			Console.WriteLine($"\n  Вызов методов:");
			simplePlugin.Execute();

			// Приведение к конкретному типу для доступа к специфичным методам
			SimplePlugin simple = (SimplePlugin)simplePlugin;
			int result = simple.Calculate(10, 20);
			Console.WriteLine($"    simple.Calculate(10, 20) = {result}");

			AdvancedPlugin advanced = (AdvancedPlugin)advancedPlugin;
			string processed = advanced.ProcessData("test");
			Console.WriteLine($"    advanced.ProcessData('test') = {processed}");

			// Использование статических методов
			Console.WriteLine($"\n  Статические методы:");
			string timestamp = Utility.GetTimestamp();
			Console.WriteLine($"    Utility.GetTimestamp(): {timestamp}");

#pragma warning disable CS0618
			string oldTimestamp = Utility.GetOldTimestamp();
#pragma warning restore CS0618
			Console.WriteLine($"    Utility.GetOldTimestamp(): {oldTimestamp} (устарел)");
		}

		static void DemonstrateReflectionExploration()
		{
			// Получаем тип сборки текущего домена приложения
			Assembly assembly = Assembly.GetAssembly(typeof(SimplePlugin));

			Console.WriteLine($"  Сборка: {assembly.GetName().Name}");
			Console.WriteLine($"  Версия: {assembly.GetName().Version}");
			Console.WriteLine($"  Расположение: {assembly.Location}");

			Console.WriteLine($"\n  ВСЕ ТИПЫ В СБОРКЕ:");
			Type[] allTypes = assembly.GetTypes();
			foreach (Type type in allTypes)
			{
				Console.WriteLine($"    {type.FullName}");
				Console.WriteLine($"      IsPublic: {type.IsPublic}, IsClass: {type.IsClass}, IsInterface: {type.IsInterface}");
			}

			Console.WriteLine($"\n  ТОЛЬКО ПУБЛИЧНЫЕ ТИПЫ:");
			Type[] publicTypes = assembly.GetExportedTypes();
			foreach (Type type in publicTypes)
			{
				Console.WriteLine($"    {type.Name} (в namespace: {type.Namespace})");
			}

			// Исследование конкретного типа
			Type pluginType = typeof(SimplePlugin);
			Console.WriteLine($"\n  ИССЛЕДОВАНИЕ ТИПА SimplePlugin:");
			Console.WriteLine($"    Полное имя: {pluginType.FullName}");
			Console.WriteLine($"    Базовый тип: {pluginType.BaseType?.Name}");
			Console.WriteLine($"    Реализует IPlugin: {typeof(IPlugin).IsAssignableFrom(pluginType)}");

			Console.WriteLine($"\n    МЕТОДЫ SimplePlugin:");
			MethodInfo[] methods = pluginType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
			foreach (MethodInfo method in methods)
			{
				Console.WriteLine($"      {method.ReturnType.Name} {method.Name}()");
			}

			Console.WriteLine($"\n    СВОЙСТВА Configuration:");
			Type configType = typeof(Configuration);
			PropertyInfo[] properties = configType.GetProperties();
			foreach (PropertyInfo prop in properties)
			{
				Console.WriteLine($"      {prop.PropertyType.Name} {prop.Name} {{ get; {(prop.CanWrite ? "set;" : "")} }}");
			}
		}

		static void DemonstrateDynamicCreation()
		{
			Type pluginType = typeof(SimplePlugin);

			Console.WriteLine($"  ДИНАМИЧЕСКОЕ СОЗДАНИЕ ЭКЗЕМПЛЯРА:");

			// Создание через Activator
			object instance = Activator.CreateInstance(pluginType);
			Console.WriteLine($"    Экземпляр создан: {instance.GetType().Name}");

			// Получение свойств через Reflection
			PropertyInfo nameProperty = pluginType.GetProperty("Name");
			string nameValue = (string)nameProperty.GetValue(instance);
			Console.WriteLine($"    Получение свойства Name: {nameValue}");

			// Вызов метода через Reflection
			Console.WriteLine($"\n  ВЫЗОВ МЕТОДОВ ЧЕРЕЗ REFLECTION:");

			MethodInfo executeMethod = pluginType.GetMethod("Execute");
			Console.Write($"    executeMethod.Invoke(): ");
			executeMethod.Invoke(instance, null);

			MethodInfo calculateMethod = pluginType.GetMethod("Calculate", new Type[] { typeof(int), typeof(int) });
			object calcResult = calculateMethod.Invoke(instance, new object[] { 15, 25 });
			Console.WriteLine($"    calculateMethod.Invoke(15, 25) = {calcResult}");

			MethodInfo getInfoMethod = pluginType.GetMethod("GetInfo");
			object infoResult = getInfoMethod.Invoke(instance, null);
			Console.WriteLine($"    getInfoMethod.Invoke() = {infoResult}");

			Console.WriteLine($"\n  ДОСТУП К ПРИВАТНЫМ ЧЛЕНАМ:");
			try
			{
				// Получение приватного метода
				MethodInfo privateMethod = pluginType.GetMethod("GetPrivateData",
					BindingFlags.NonPublic | BindingFlags.Instance);

				if (privateMethod != null)
				{
					object privateResult = privateMethod.Invoke(instance, null);
					Console.WriteLine($"    Приватный метод GetPrivateData() = {privateResult}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка доступа к приватному методу: {ex.Message}");
			}

			Console.WriteLine($"\n  РАБОТА С GENERIC ТИПАМИ:");
			Type repositoryType = typeof(Repository<>);

			try
			{
				// Используем тип с конструктором по умолчанию
				Type dataPointType = typeof(DataPoint);
				Type dataPointRepositoryType = repositoryType.MakeGenericType(dataPointType);

				Console.WriteLine($"    Создание Repository<DataPoint>...");
				object repository = Activator.CreateInstance(dataPointRepositoryType);
				Console.WriteLine($"    Repository<DataPoint> создан");

				// Создаем DataPoint
				object dataPoint = Activator.CreateInstance(dataPointType, 10, 20, "Point1");

				// Вызываем метод Add
				MethodInfo addMethod = dataPointRepositoryType.GetMethod("Add");
				addMethod.Invoke(repository, new object[] { dataPoint });
				Console.WriteLine($"    Добавлен DataPoint в репозиторий");

				// Получаем свойство Count
				PropertyInfo countProperty = dataPointRepositoryType.GetProperty("Count");
				int count = (int)countProperty.GetValue(repository);
				Console.WriteLine($"    Repository<DataPoint>.Count = {count}");

				// Вызываем метод Get для получения элемента
				MethodInfo getMethod = dataPointRepositoryType.GetMethod("Get");
				object retrievedItem = getMethod.Invoke(repository, new object[] { 0 });
				Console.WriteLine($"    Получен элемент из репозитория");

				// Получаем информацию о DataPoint
				MethodInfo getInfoMethodDP = dataPointType.GetMethod("GetInfo");
				string info = (string)getInfoMethodDP.Invoke(retrievedItem, null);
				Console.WriteLine($"    DataPoint.GetInfo() = {info}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка при работе с generic типами: {ex.Message}");

				// Альтернативный вариант с другим типом
				try
				{
					Console.WriteLine($"\n    Альтернативный вариант:");
					Type listType = typeof(List<>);
					Type stringListType = listType.MakeGenericType(typeof(string));
					object stringList = Activator.CreateInstance(stringListType);

					MethodInfo addMethod = stringListType.GetMethod("Add");
					addMethod.Invoke(stringList, new object[] { "Test Item" });

					PropertyInfo countProperty = stringListType.GetProperty("Count");
					int count = (int)countProperty.GetValue(stringList);
					Console.WriteLine($"    List<string>.Count = {count}");
				}
				catch (Exception ex2)
				{
					Console.WriteLine($"    Ошибка в альтернативном варианте: {ex2.Message}");
				}
			}

			Console.WriteLine($"\n  РАБОТА СО СТАТИЧЕСКИМИ МЕТОДАМИ:");
			Type calculatorType = typeof(Calculator);

			// Создание экземпляра Calculator для нестатических методов
			object calculatorInstance = Activator.CreateInstance(calculatorType);

			// Вызов нестатического метода
			MethodInfo addMethodInstance = calculatorType.GetMethod("Add", new Type[] { typeof(int), typeof(int) });
			object addResult = addMethodInstance.Invoke(calculatorInstance, new object[] { 5, 3 });
			Console.WriteLine($"    calculator.Add(5, 3) = {addResult}");

			// Вызов статического метода
			MethodInfo multiplyMethod = calculatorType.GetMethod("Multiply", BindingFlags.Public | BindingFlags.Static);
			object multiplyResult = multiplyMethod.Invoke(null, new object[] { 4, 6 });
			Console.WriteLine($"    Calculator.Multiply(4, 6) = {multiplyResult}");

			// Вызов перегруженного метода
			MethodInfo addDoubleMethod = calculatorType.GetMethod("Add", new Type[] { typeof(double), typeof(double) });
			object addDoubleResult = addDoubleMethod.Invoke(calculatorInstance, new object[] { 2.5, 3.5 });
			Console.WriteLine($"    calculator.Add(2.5, 3.5) = {addDoubleResult}");

			Console.WriteLine($"\n  РАБОТА С ПАРАМЕТРАМИ ПО УМОЛЧАНИЮ:");
			MethodInfo formatMethod = calculatorType.GetMethod("Format", new Type[] { typeof(string), typeof(bool) });

			// Вызов с двумя параметрами
			object formatResult1 = formatMethod.Invoke(calculatorInstance, new object[] { "hello", true });
			Console.WriteLine($"    calculator.Format('hello', true) = {formatResult1}");

			// Вызов с одним параметром (используется значение по умолчанию)
			object formatResult2 = formatMethod.Invoke(calculatorInstance, new object[] { "hello", Type.Missing });
			Console.WriteLine($"    calculator.Format('hello') = {formatResult2}");
		}

		static void DemonstrateAttributeReading()
		{
			Type simplePluginType = typeof(SimplePlugin);
			Type advancedPluginType = typeof(AdvancedPlugin);

			Console.WriteLine($"  ЧТЕНИЕ АТРИБУТОВ:");

			// Получение атрибутов типа
			PluginAttribute simpleAttr = (PluginAttribute)Attribute.GetCustomAttribute(simplePluginType, typeof(PluginAttribute));
			PluginAttribute advancedAttr = (PluginAttribute)Attribute.GetCustomAttribute(advancedPluginType, typeof(PluginAttribute));

			if (simpleAttr != null)
			{
				Console.WriteLine($"    SimplePlugin атрибут:");
				Console.WriteLine($"      Description: {simpleAttr.Description}");
				Console.WriteLine($"      Version: {simpleAttr.Version}");
			}

			if (advancedAttr != null)
			{
				Console.WriteLine($"    AdvancedPlugin атрибут:");
				Console.WriteLine($"      Description: {advancedAttr.Description}");
				Console.WriteLine($"      Version: {advancedAttr.Version}");
			}

			Console.WriteLine($"\n  ПОИСК ВСЕХ ТИПОВ С АТРИБУТОМ PluginAttribute:");
			Assembly assembly = Assembly.GetAssembly(typeof(SimplePlugin));

			foreach (Type type in assembly.GetTypes())
			{
				PluginAttribute attr = (PluginAttribute)Attribute.GetCustomAttribute(type, typeof(PluginAttribute));
				if (attr != null)
				{
					Console.WriteLine($"    Найден: {type.Name}");
					Console.WriteLine($"      Description: {attr.Description}");
					Console.WriteLine($"      Version: {attr.Version}");
				}
			}

			Console.WriteLine($"\n  ЧТЕНИЕ АТРИБУТА Obsolete:");
			Type utilityType = typeof(Utility);
			MethodInfo oldMethod = utilityType.GetMethod("GetOldTimestamp");
			ObsoleteAttribute obsoleteAttr = (ObsoleteAttribute)Attribute.GetCustomAttribute(oldMethod, typeof(ObsoleteAttribute));

			if (obsoleteAttr != null)
			{
				Console.WriteLine($"    Метод GetOldTimestamp помечен как устаревший");
				Console.WriteLine($"      Message: {obsoleteAttr.Message}");
				Console.WriteLine($"      IsError: {obsoleteAttr.IsError}");
			}
		}

		static void ComparePerformance()
		{
			const int iterations = 100000;

			Console.WriteLine($"  Сравнение {iterations:N0} вызовов:");

			// Прямой вызов
			SimplePlugin plugin = new SimplePlugin();

			var directStart = DateTime.Now;
			for (int i = 0; i < iterations; i++)
			{
				int result = plugin.Calculate(10, 20);
			}
			var directTime = DateTime.Now - directStart;
			Console.WriteLine($"    Прямой вызов: {directTime.TotalMilliseconds:F2} ms");

			// Вызов через Reflection
			Type pluginType = typeof(SimplePlugin);
			object instance = Activator.CreateInstance(pluginType);
			MethodInfo method = pluginType.GetMethod("Calculate", new Type[] { typeof(int), typeof(int) });

			var reflectStart = DateTime.Now;
			for (int i = 0; i < iterations; i++)
			{
				object result = method.Invoke(instance, new object[] { 10, 20 });
			}
			var reflectTime = DateTime.Now - reflectStart;
			Console.WriteLine($"    Reflection вызов: {reflectTime.TotalMilliseconds:F2} ms");

			Console.WriteLine($"    Reflection медленнее в {reflectTime.TotalMilliseconds / directTime.TotalMilliseconds:F1} раз");

			// Демонстрация поиска всех плагинов через интерфейс
			Console.WriteLine($"\n  ОБНАРУЖЕНИЕ ПЛАГИНОВ:");
			Assembly assembly = Assembly.GetAssembly(typeof(SimplePlugin));
			List<IPlugin> plugins = new List<IPlugin>();

			foreach (Type type in assembly.GetTypes())
			{
				if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
				{
					Console.WriteLine($"    Найден плагин: {type.Name}");
					IPlugin pluginInstance = (IPlugin)Activator.CreateInstance(type);
					plugins.Add(pluginInstance);
				}
			}

			Console.WriteLine($"\n  ИСПОЛЬЗОВАНИЕ НАЙДЕННЫХ ПЛАГИНОВ:");
			foreach (var p in plugins)
			{
				Console.WriteLine($"    {p.Name}: {p.GetInfo()}");
			}
		}
	}
}