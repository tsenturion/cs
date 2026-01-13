using System;

// Пространство имен - часть дизайна библиотеки
// Хорошо спроектированные пространства имен делают библиотеку понятной
namespace ClassLibrary1.Core
{
	// Интерфейс - лучший способ определения контракта
	// Потребители зависят от интерфейса, а не от реализации
	public interface ICalculator
	{
		// Метод интерфейса - чистая часть контракта
		int Calculate(int a, int b);
	}

	// Базовый абстрактный класс - ещё один способ определения контракта
	public abstract class BaseProcessor
	{
		// Абстрактный метод - должен быть реализован в производных классах
		public abstract string Process(string input);

		// Виртуальный метод - может быть переопределен
		public virtual string GetVersion()
		{
			return "1.0.0.0";
		}
	}
}

namespace ClassLibrary1.Implementations
{
	// Конкретная реализация интерфейса
	// Можно заменить на другую реализацию без изменения контракта
	public class SimpleCalculator : Core.ICalculator
	{
		// Реализация контракта интерфейса
		public int Calculate(int a, int b)
		{
			return a + b;
		}

		// Новый метод - добавлен в новой версии
		// Показывает эволюцию API
		public int Multiply(int a, int b)
		{
			return a * b;
		}
	}

	// Ещё одна реализация того же интерфейса
	// Демонстрирует возможность разных реализаций
	public class AdvancedCalculator : Core.ICalculator
	{
		public int Calculate(int a, int b)
		{
			// Сложная реализация может меняться без влияния на контракт
			return InternalComplexCalculation(a, b);
		}

		// Internal метод - детали реализации
		internal int InternalComplexCalculation(int x, int y)
		{
			return (x * x) + (y * y);
		}
	}
}

namespace ClassLibrary1.Utilities
{
	// Public класс - часть публичного API
	// Изменения в публичных членах влияют на совместимость
	public class StringFormatter
	{
		// Public метод - фиксированный контракт
		public string Format(string input)
		{
			// Использование internal метода безопасно
			return InternalFormatLogic(input);
		}

		// Internal метод - может меняться свободно
		internal string InternalFormatLogic(string text)
		{
			return $"Formatted: {text}";
		}

		// Метод с параметрами по умолчанию - показывает эволюцию API
		// Новые параметры можно добавлять без нарушения существующего кода
		public string FormatWithOptions(string input, bool uppercase = false)
		{
			var result = InternalFormatLogic(input);
			return uppercase ? result.ToUpper() : result;
		}
	}
}

namespace ClassLibrary1.Internal
{
	// Internal класс - не виден за пределами сборки
	// Полная свобода изменений
	internal class ConfigurationManager
	{
		internal string GetSetting(string key)
		{
			return "Internal configuration value";
		}
	}

	// Internal структура - тоже скрыта от внешнего мира
	internal struct InternalData
	{
		public int Value;
		public string Name;
	}
}

// Глобальный класс в корневом namespace
// Демонстрирует организацию кода по namespace
public class GlobalUtility
{
	// Статический член - тоже часть публичного API
	public static string GetGlobalMessage()
	{
		return "Global utility method";
	}

	// Метод, помеченный как устаревший
	// Показывает управление жизненным циклом API
	[Obsolete("Use GetGlobalMessage() instead", false)]
	public static string OldGetMessage()
	{
		return "Deprecated method";
	}

	// Метод с устареванием и ошибкой
	[Obsolete("This method will be removed in version 2.0", true)]
	public static string ToBeRemoved()
	{
		return "Will cause compilation error if used";
	}
}