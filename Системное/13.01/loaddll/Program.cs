using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace LibraryConsumer
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("=== ИСПОЛЬЗОВАНИЕ БИБЛИОТЕКИ DLL ===\n");

			// Способ 1: Статическое подключение (ссылка на проект)
			Console.WriteLine("1. СТАТИЧЕСКОЕ ПОДКЛЮЧЕНИЕ (ссылка на проект):");
			DemonstrateStaticReference();

			// Способ 2: Динамическая загрузка (готовый DLL файл)
			Console.WriteLine("\n2. ДИНАМИЧЕСКАЯ ЗАГРУЗКА (готовый DLL):");
			DemonstrateDynamicLoading();

			// Исследование метаданных
			Console.WriteLine("\n3. ИССЛЕДОВАНИЕ МЕТАДАННЫХ:");
			ExploreAssemblyMetadata();
		}

		static void DemonstrateStaticReference()
		{
			// Компилятор знает типы из метаданных DLL
			// IntelliSense работает, проверка типов на этапе компиляции

			// Использование интерфейса - зависимость от контракта
			ClassLibrary1.Core.ICalculator calculator = new ClassLibrary1.Implementations.SimpleCalculator();

			// Вызов методов публичного API
			int result = calculator.Calculate(10, 20);
			Console.WriteLine($"  Использование интерфейса: ICalculator.Calculate(10, 20) = {result}");

			// Использование конкретного класса из публичного API
			var formatter = new ClassLibrary1.Utilities.StringFormatter();
			string formatted = formatter.Format("test data");
			Console.WriteLine($"  Использование класса: StringFormatter.Format('test data') = '{formatted}'");

			// Использование метода с параметрами по умолчанию
			string upperFormatted = formatter.FormatWithOptions("hello", true);
			Console.WriteLine($"  Метод с параметрами по умолчанию: FormatWithOptions('hello', true) = '{upperFormatted}'");

			// Глобальный класс
			string globalMessage = GlobalUtility.GetGlobalMessage();
			Console.WriteLine($"  Глобальный класс: GlobalUtility.GetGlobalMessage() = '{globalMessage}'");

			// Устаревший метод (показывает предупреждение при компиляции)
#pragma warning disable CS0618
			string oldMessage = GlobalUtility.OldGetMessage();
#pragma warning restore CS0618
			Console.WriteLine($"  Устаревший метод: GlobalUtility.OldGetMessage() = '{oldMessage}'");

			// Другая реализация того же интерфейса
			ClassLibrary1.Core.ICalculator advancedCalc = new ClassLibrary1.Implementations.AdvancedCalculator();
			int advancedResult = advancedCalc.Calculate(3, 4);
			Console.WriteLine($"  Другая реализация: AdvancedCalculator.Calculate(3, 4) = {advancedResult}");
		}

		static void DemonstrateDynamicLoading()
		{
			try
			{
				// Путь к DLL - CLR ищет в стандартных местах
				string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClassLibrary1.dll");

				Console.WriteLine($"  Путь поиска DLL: {dllPath}");

				if (!File.Exists(dllPath))
				{
					Console.WriteLine($"  ОШИБКА: Файл не найден!");
					Console.WriteLine($"  CLR проверяет: 1) Директорию с .exe 2) GAC 3) Указанные пути");
					return;
				}

				// Загрузка сборки в домен приложения
				Assembly assembly = Assembly.LoadFrom(dllPath);

				// Информация о сборке
				AssemblyName assemblyName = assembly.GetName();
				Console.WriteLine($"  Сборка загружена: {assemblyName.Name}");
				Console.WriteLine($"  Версия: {assemblyName.Version}");
				Console.WriteLine($"  Архитектура: {assemblyName.ProcessorArchitecture}");

				// Получение типа по полному имени
				Type calculatorType = assembly.GetType("ClassLibrary1.Implementations.SimpleCalculator");
				if (calculatorType == null)
				{
					Console.WriteLine($"  Тип не найден в метаданных!");
					return;
				}

				// Создание экземпляра
				object instance = Activator.CreateInstance(calculatorType);

				// Поиск интерфейса в метаданных
				Type interfaceType = assembly.GetType("ClassLibrary1.Core.ICalculator");

				if (interfaceType != null && interfaceType.IsAssignableFrom(calculatorType))
				{
					// Получение метода из метаданных
					MethodInfo calculateMethod = interfaceType.GetMethod("Calculate");

					// Вызов метода через рефлексию
					object result = calculateMethod.Invoke(instance, new object[] { 15, 25 });
					Console.WriteLine($"  Вызов через рефлексию: Calculate(15, 25) = {result}");
				}

				// Проверка нового метода в SimpleCalculator
				MethodInfo multiplyMethod = calculatorType.GetMethod("Multiply");
				if (multiplyMethod != null)
				{
					object multiplyResult = multiplyMethod.Invoke(instance, new object[] { 6, 7 });
					Console.WriteLine($"  Новый метод: Multiply(6, 7) = {multiplyResult}");
				}

			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine($"  FileNotFoundException: {ex.Message}");
				Console.WriteLine($"  Ошибка разрешения зависимостей CLR");
			}
			catch (BadImageFormatException ex)
			{
				Console.WriteLine($"  BadImageFormatException: {ex.Message}");
				Console.WriteLine($"  Несовместимость версий .NET");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка: {ex.GetType().Name}: {ex.Message}");
			}
		}

		static void ExploreAssemblyMetadata()
		{
			try
			{
				string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClassLibrary1.dll");
				Assembly assembly = Assembly.LoadFrom(dllPath);

				Console.WriteLine($"\n  МЕТАДАННЫЕ СБОРКИ:");

				// Информация о версии из метаданных
				AssemblyName nameInfo = assembly.GetName();
				Console.WriteLine($"    Имя сборки: {nameInfo.Name}");
				Console.WriteLine($"    Полное имя: {nameInfo.FullName}");
				Console.WriteLine($"    Версия: {nameInfo.Version}");
				Console.WriteLine($"    PublicKeyToken: {BitConverter.ToString(nameInfo.GetPublicKeyToken() ?? new byte[0])}");

				// Целевая платформа из метаданных
				var targetFramework = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
				if (targetFramework != null)
				{
					Console.WriteLine($"    Целевой фреймворк: {targetFramework.FrameworkName}");
				}

				// Публичные типы (публичный API)
				Console.WriteLine($"\n  ПУБЛИЧНЫЕ ТИПЫ (public API):");
				Type[] exportedTypes = assembly.GetExportedTypes();

				foreach (Type type in exportedTypes)
				{
					Console.WriteLine($"\n    Тип: {type.FullName}");
					Console.WriteLine($"      Namespace: {type.Namespace}");
					Console.WriteLine($"      IsInterface: {type.IsInterface}");
					Console.WriteLine($"      IsClass: {type.IsClass}");

					// Публичные методы
					MethodInfo[] publicMethods = type.GetMethods(
						BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

					if (publicMethods.Length > 0)
					{
						Console.WriteLine($"      Публичные методы:");
						foreach (MethodInfo method in publicMethods)
						{
							// Проверка атрибутов
							var obsoleteAttr = method.GetCustomAttribute<ObsoleteAttribute>();
							string obsoleteMarker = obsoleteAttr != null ? " [УСТАРЕЛ]" : "";

							// Параметры метода
							string parameters = "(";
							ParameterInfo[] paramInfos = method.GetParameters();
							for (int i = 0; i < paramInfos.Length; i++)
							{
								parameters += $"{paramInfos[i].ParameterType.Name} {paramInfos[i].Name}";
								if (i < paramInfos.Length - 1) parameters += ", ";
							}
							parameters += ")";

							Console.WriteLine($"        - {method.ReturnType.Name} {method.Name}{parameters}{obsoleteMarker}");
						}
					}
				}

				// Internal типы (не видны через GetExportedTypes)
				Console.WriteLine($"\n  INTERNAL ТИПЫ:");
				Console.WriteLine($"    Недоступны через публичное API");
				Console.WriteLine($"    Видны только внутри сборки ClassLibrary1");

				// Зависимости сборки
				Console.WriteLine($"\n  ЗАВИСИМОСТИ:");
				AssemblyName[] references = assembly.GetReferencedAssemblies();
				foreach (var reference in references)
				{
					Console.WriteLine($"    - {reference.Name} v{reference.Version}");
				}

				// Проверка файла DLL
				FileInfo fileInfo = new FileInfo(dllPath);
				Console.WriteLine($"\n  ФАЙЛ DLL:");
				Console.WriteLine($"    Размер: {fileInfo.Length} байт");
				Console.WriteLine($"    Дата создания: {fileInfo.CreationTime}");
				Console.WriteLine($"    Дата изменения: {fileInfo.LastWriteTime}");

			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка при исследовании: {ex.Message}");
			}
		}
	}
}