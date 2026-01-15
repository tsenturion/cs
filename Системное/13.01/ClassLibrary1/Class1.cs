using System;
using System.Collections.Generic;

namespace DynamicLoadingDemo
{
	// Общие контракты - будут известны и хосту, и плагинам
	public interface IPlugin
	{
		string Name { get; }
		string Version { get; }
		void Initialize();
		string Process(string input);
	}

	public interface IDataProcessor
	{
		string ProcessData(object data);
		bool CanProcess(Type dataType);
	}

	// Атрибут для маркировки плагинов
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class PluginMetadataAttribute : Attribute
	{
		public string Author { get; }
		public string Description { get; }
		public string RequiredHostVersion { get; }

		public PluginMetadataAttribute(string author, string description, string requiredHostVersion = "1.0.0")
		{
			Author = author;
			Description = description;
			RequiredHostVersion = requiredHostVersion;
		}
	}

	// Базовый класс для плагинов с общими функциями
	public abstract class PluginBase
	{
		protected bool IsInitialized { get; private set; }

		public virtual void Initialize()
		{
			IsInitialized = true;
		}

		protected void EnsureInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Плагин не инициализирован");
		}
	}

	// Пример плагина версии 1.0
	[PluginMetadata("Company A", "Плагин для обработки текста", "1.0.0")]
	public class TextProcessorPlugin : PluginBase, IPlugin
	{
		public string Name => "Text Processor";
		public string Version => "1.0.0";

		private List<string> processedItems = new List<string>();

		public override void Initialize()
		{
			base.Initialize();
			Console.WriteLine($"[{Name}] Инициализация плагина текстовой обработки");
		}

		public string Process(string input)
		{
			EnsureInitialized();

			string result = $"Обработан текст: {input.ToUpper()}";
			processedItems.Add(result);
			return result;
		}

		public int GetProcessedCount() => processedItems.Count;
	}

	// Пример плагина версии 2.0 с новым функционалом
	[PluginMetadata("Company B", "Расширенный плагин обработки", "1.1.0")]
	public class AdvancedDataPlugin : PluginBase, IPlugin, IDataProcessor
	{
		public string Name => "Advanced Data Processor";
		public string Version => "2.0.0";

		public override void Initialize()
		{
			base.Initialize();
			Console.WriteLine($"[{Name}] Инициализация расширенного плагина");
			LoadConfiguration();
		}

		private void LoadConfiguration()
		{
			// Симуляция загрузки конфигурации
			Console.WriteLine($"[{Name}] Загрузка конфигурации...");
		}

		public string Process(string input)
		{
			EnsureInitialized();

			if (string.IsNullOrEmpty(input))
				throw new ArgumentException("Входные данные не могут быть пустыми");

			return $"Расширенная обработка: {input} (длина: {input.Length})";
		}

		public string ProcessData(object data)
		{
			if (data == null)
				return "Данные отсутствуют";

			return $"Обработаны данные типа {data.GetType().Name}: {data}";
		}

		public bool CanProcess(Type dataType)
		{
			return dataType != null && dataType != typeof(void);
		}

		// Новый метод в версии 2.0
		public string Analyze(string data)
		{
			return $"Анализ данных: сложность = {data.Length * 0.5:F1}";
		}
	}

	// Плагин с зависимостью от сторонней библиотеки (симуляция)
	[PluginMetadata("Company C", "Математический плагин с зависимостями")]
	public class MathPlugin : PluginBase, IPlugin
	{
		public string Name => "Math Processor";
		public string Version => "1.5.0";

		public override void Initialize()
		{
			base.Initialize();

			// Симуляция проверки зависимостей
			if (!CheckDependencies())
				throw new InvalidOperationException("Не все зависимости удовлетворены");
		}

		private bool CheckDependencies()
		{
			// В реальном приложении здесь проверялись бы версии DLL
			return true;
		}

		public string Process(string input)
		{
			EnsureInitialized();

			if (double.TryParse(input, out double number))
			{
				double result = Math.Sqrt(Math.Abs(number));
				return $"√|{number}| = {result:F4}";
			}

			return $"Не удалось распознать число: {input}";
		}

		// Специфичные для математического плагина методы
		public double CalculateExpression(string expression)
		{
			// Упрощённый пример
			return expression.Length * 2.5;
		}
	}

	// Плагин с ошибкой - для демонстрации обработки исключений
	[PluginMetadata("Unknown", "Проблемный плагин")]
	public class BuggyPlugin : IPlugin
	{
		public string Name => "Buggy Plugin";
		public string Version => "0.1.0";

		private bool initialized = false;

		public void Initialize()
		{
			if (DateTime.Now.Second % 3 == 0) // Случайная ошибка
				throw new InvalidOperationException("Случайная ошибка при инициализации");

			initialized = true;
		}

		public string Process(string input)
		{
			if (!initialized)
				throw new InvalidOperationException("Плагин не инициализирован");

			if (input == "crash")
				throw new ArgumentException("Намеренная ошибка обработки");

			return $"Обработано: {input}";
		}
	}

	// Класс с общими утилитами, которые могут использоваться хостами
	public static class PluginHostUtilities
	{
		public static bool ValidatePluginVersion(string pluginVersion, string hostVersion)
		{
			try
			{
				var pluginVer = new Version(pluginVersion);
				var hostVer = new Version(hostVersion);

				// Простая проверка: плагин должен быть не старше хоста
				return pluginVer <= hostVer;
			}
			catch
			{
				return false;
			}
		}

		public static string GetPluginInfo(IPlugin plugin)
		{
			return $"{plugin.Name} v{plugin.Version} (инициализирован: {plugin != null})";
		}
	}

	// Пример конфигурации плагина
	public class PluginConfiguration
	{
		public string PluginPath { get; set; }
		public bool AutoInitialize { get; set; }
		public string[] AllowedAuthors { get; set; }
		public TimeSpan Timeout { get; set; }

		public PluginConfiguration()
		{
			AutoInitialize = true;
			AllowedAuthors = new[] { "Company A", "Company B", "Company C" };
			Timeout = TimeSpan.FromSeconds(30);
		}
	}

	// Класс для демонстрации передачи сложных данных
	public class ComplexData
	{
		public Guid Id { get; } = Guid.NewGuid();
		public DateTime Timestamp { get; } = DateTime.Now;
		public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

		public ComplexData(params (string key, object value)[] properties)
		{
			foreach (var (key, value) in properties)
			{
				Properties[key] = value;
			}
		}

		public override string ToString()
		{
			return $"ComplexData[{Id}]: {Properties.Count} свойств";
		}
	}
}