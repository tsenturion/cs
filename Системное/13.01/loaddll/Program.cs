using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace LoadDLL
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("=== Динамическая загрузка DLL ===\n");

			// 1. Динамическая загрузка DLL
			Console.WriteLine("1. Загрузка ClassLibrary1.dll:");
			LoadAndUseDLL();

			// 2. Проверка всех методов
			Console.WriteLine("\n2. Исследование типов библиотеки:");
			ExploreAssembly();
		}

		static void LoadAndUseDLL()
		{
			try
			{
				// Получаем путь к текущей директории
				string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
				string dllPath = Path.Combine(currentDirectory, "ClassLibrary1.dll");

				Console.WriteLine($"  Ищу DLL по пути: {dllPath}");

				// Проверяем существование файла
				if (!File.Exists(dllPath))
				{
					Console.WriteLine($"  Файл не найден!");
					Console.WriteLine($"  Содержимое папки {currentDirectory}:");
					foreach (var file in Directory.GetFiles(currentDirectory, "*.dll"))
					{
						Console.WriteLine($"    - {Path.GetFileName(file)}");
					}
					return;
				}

				Console.WriteLine($"  Файл найден, размер: {new FileInfo(dllPath).Length} байт");

				// Способ 1: LoadFrom (рекомендуется)
				Assembly assembly = Assembly.LoadFrom(dllPath);
				Console.WriteLine($"  Сборка загружена: {assembly.FullName}");

				// Получаем тип Class1
				Type class1Type = assembly.GetType("ClassLibrary1.Class1");
				if (class1Type == null)
				{
					Console.WriteLine("  Тип ClassLibrary1.Class1 не найден!");

					// Показываем все типы в сборке
					Console.WriteLine("  Доступные типы в сборке:");
					foreach (Type type in assembly.GetTypes())
					{
						Console.WriteLine($"    - {type.FullName}");
					}
					return;
				}

				// Создаем экземпляр
				object instance = Activator.CreateInstance(class1Type);
				Console.WriteLine("  Экземпляр класса создан");

				// Вызываем метод GetMessage
				MethodInfo getMessageMethod = class1Type.GetMethod("GetMessage");
				if (getMessageMethod != null)
				{
					string message = (string)getMessageMethod.Invoke(instance, null);
					Console.WriteLine($"  GetMessage(): {message}");
				}
				else
				{
					Console.WriteLine("  Метод GetMessage не найден!");

					// Показываем все методы
					Console.WriteLine("  Доступные методы:");
					foreach (MethodInfo method in class1Type.GetMethods())
					{
						Console.WriteLine($"    - {method.ReturnType.Name} {method.Name}()");
					}
				}

				// Вызываем метод Calculate
				MethodInfo calculateMethod = class1Type.GetMethod("Calculate");
				if (calculateMethod != null)
				{
					object[] parameters = { 25, 17 };
					int result = (int)calculateMethod.Invoke(instance, parameters);
					Console.WriteLine($"  Calculate(25, 17): {result}");
				}

				// Альтернативный способ с dynamic
				Console.WriteLine("\n  Использование dynamic:");
				try
				{
					dynamic dynamicInstance = Activator.CreateInstance(class1Type);
					Console.WriteLine($"    dynamic GetMessage(): {dynamicInstance.GetMessage()}");
					Console.WriteLine($"    dynamic Calculate(100, 200): {dynamicInstance.Calculate(100, 200)}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    Ошибка dynamic: {ex.Message}");
				}

			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine($"  FileNotFoundException: {ex.Message}");
			}
			catch (BadImageFormatException ex)
			{
				Console.WriteLine($"  BadImageFormatException: {ex.Message}");
				Console.WriteLine("  Возможно, DLL скомпилирована для другой версии .NET");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка: {ex.GetType().Name}: {ex.Message}");
				Console.WriteLine($"  StackTrace: {ex.StackTrace}");
			}
		}

		static void ExploreAssembly()
		{
			try
			{
				string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClassLibrary1.dll");

				if (!File.Exists(dllPath))
				{
					Console.WriteLine($"  DLL не найдена: {dllPath}");
					return;
				}

				// Загружаем сборку
				Assembly assembly = Assembly.LoadFrom(dllPath);

				Console.WriteLine($"  Информация о сборке:");
				AssemblyName assemblyName = assembly.GetName();
				Console.WriteLine($"    Имя: {assemblyName.Name}");
				Console.WriteLine($"    Версия: {assemblyName.Version}");
				Console.WriteLine($"    Архитектура: {assemblyName.ProcessorArchitecture}");
				Console.WriteLine($"    Расположение: {assembly.Location}");

				// Проверяем целевую платформу
				Console.WriteLine($"\n  Целевая платформа:");
				var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
				if (targetFrameworkAttribute != null)
				{
					Console.WriteLine($"    {targetFrameworkAttribute.FrameworkName}");
				}

				// Получаем все типы
				Console.WriteLine($"\n  Типы в сборке:");
				Type[] types = assembly.GetTypes();
				if (types.Length == 0)
				{
					Console.WriteLine($"    (нет типов)");
				}

				foreach (Type type in types)
				{
					Console.WriteLine($"    Тип: {type.FullName}");
					Console.WriteLine($"      Namespace: {type.Namespace}");
					Console.WriteLine($"      IsPublic: {type.IsPublic}");
					Console.WriteLine($"      IsClass: {type.IsClass}");

					// Показываем методы
					MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (methods.Length > 0)
					{
						Console.WriteLine($"      Методы:");
						foreach (MethodInfo method in methods)
						{
							string parameters = "";
							var paramInfos = method.GetParameters();
							if (paramInfos.Length > 0)
							{
								parameters = "(" + string.Join(", ", paramInfos.Select(p => $"{p.ParameterType.Name} {p.Name}")) + ")";
							}
							else
							{
								parameters = "()";
							}
							Console.WriteLine($"        - {method.ReturnType.Name} {method.Name}{parameters}");
						}
					}
				}

				// Проверка зависимостей
				Console.WriteLine($"\n  Зависимости сборки:");
				AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
				if (referencedAssemblies.Length == 0)
				{
					Console.WriteLine($"    (нет зависимостей)");
				}
				else
				{
					foreach (AssemblyName referencedAssembly in referencedAssemblies)
					{
						Console.WriteLine($"    - {referencedAssembly.Name} v{referencedAssembly.Version}");
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка при исследовании: {ex.GetType().Name}: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"  Внутренняя ошибка: {ex.InnerException.Message}");
				}
			}
		}
	}
}