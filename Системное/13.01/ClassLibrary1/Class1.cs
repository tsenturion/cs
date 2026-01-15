using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionDemoLibrary
{
	// Интерфейс для демонстрации контрактов
	public interface IPlugin
	{
		string Name { get; }
		void Execute();
		string GetInfo();
	}

	// Атрибут для метаданных - может быть прочитан через Reflection
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class PluginAttribute : Attribute
	{
		public string Description { get; }
		public string Version { get; }

		public PluginAttribute(string description, string version = "1.0.0")
		{
			Description = description;
			Version = version;
		}
	}

	// Публичные классы - доступны через Reflection
	[Plugin("Базовая реализация плагина", "1.0.0")]
	public class SimplePlugin : IPlugin
	{
		public string Name => "Simple Plugin";

		public void Execute()
		{
			Console.WriteLine($"[{Name}] Выполнение через прямой вызов");
		}

		public string GetInfo()
		{
			return $"Плагин: {Name}, Версия: 1.0";
		}

		// Публичный метод с параметрами
		public int Calculate(int a, int b)
		{
			return a + b;
		}

		// Приватный метод - не виден через обычный Reflection без флагов
		private string GetPrivateData()
		{
			return "Секретные данные";
		}

		// Internal метод - виден только внутри сборки
		internal string GetInternalData()
		{
			return "Внутренние данные";
		}
	}

	// Ещё один плагин с другим атрибутом
	[Plugin("Расширенная версия плагина", "2.0.0")]
	public class AdvancedPlugin : IPlugin
	{
		public string Name => "Advanced Plugin";

		public void Execute()
		{
			Console.WriteLine($"[{Name}] Расширенное выполнение");
		}

		public string GetInfo()
		{
			return $"Продвинутый плагин версии 2.0";
		}

		// Новый метод в расширенной версии
		public string ProcessData(string input)
		{
			return $"Обработано: {input.ToUpper()}";
		}
	}

	// Класс с несколькими методами для демонстрации Reflection
	public class Calculator
	{
		public int Add(int a, int b) => a + b;
		public int Subtract(int a, int b) => a - b;
		public static int Multiply(int a, int b) => a * b;

		// Перегруженные методы
		public double Add(double a, double b) => a + b;

		// Метод с параметрами по умолчанию
		public string Format(string text, bool uppercase = false)
		{
			return uppercase ? text.ToUpper() : text;
		}

		// Приватный метод
		private int InternalCalculation() => 42;
	}

	// Структура для демонстрации работы с разными типами
	public struct DataPoint
	{
		public int X;
		public int Y;
		public string Label;

		public DataPoint(int x, int y, string label)
		{
			X = x;
			Y = y;
			Label = label;
		}

		public string GetInfo() => $"{Label}: ({X}, {Y})";
	}

	// Generic класс для демонстрации работы с дженериками через Reflection
	public class Repository<T> where T : new()
	{
		private List<T> items = new List<T>();

		public void Add(T item)
		{
			items.Add(item);
		}

		public T Get(int index)
		{
			return items[index];
		}

		public int Count => items.Count;
	}

	// Класс с событиями для демонстрации работы с событиями через Reflection
	public class EventPublisher
	{
		public event EventHandler<string> DataProcessed;

		public void ProcessData(string data)
		{
			Console.WriteLine($"Обработка данных: {data}");
			DataProcessed?.Invoke(this, $"Обработано: {data}");
		}
	}

	// Класс с свойствами разных типов
	public class Configuration
	{
		public string Name { get; set; }
		public int MaxConnections { get; set; }
		public bool Enabled { get; set; }

		public string ReadOnlyProperty => "Read Only Value";

		private string secret = "Секретное значение";
		public string Secret
		{
			get => secret;
			private set => secret = value;
		}

		public void SetSecret(string value) => Secret = value;
	}

	// Internal класс - не доступен извне через обычный Reflection
	internal class InternalComponent
	{
		public void DoWork()
		{
			Console.WriteLine("Внутренняя работа");
		}
	}

	// Статический класс
	public static class Utility
	{
		public static string GetTimestamp()
		{
			return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}

		[Obsolete("Используйте новый метод GetTimestamp()", false)]
		public static string GetOldTimestamp()
		{
			return DateTime.Now.ToString();
		}
	}
}