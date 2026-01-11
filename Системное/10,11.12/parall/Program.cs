using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
	static void Main()
	{
		// =========================
		// ШАГ 1. Подготовка данных
		// =========================

		// Создаём коллекцию чисел от 1 до 10 000 000
		// Эти данные будут использоваться на всех следующих шагах
		List<int> numbers = new List<int>(10_000_000);

		for (int i = 1; i <= 10_000_000; i++)
		{
			numbers.Add(i);
		}

		Console.WriteLine($"Количество элементов: {numbers.Count}");
		Console.WriteLine("Подготовка данных завершена.");
		Console.WriteLine();

		// =========================
		// ШАГ 2. Последовательное решение
		// =========================

		// Stopwatch используем для измерения времени выполнения
		Stopwatch stopwatch = Stopwatch.StartNew();

		long sum = 0;

		// Обычный последовательный проход по коллекции
		foreach (int number in numbers)
		{
			sum += HeavyCalculation(number);
		}

		stopwatch.Stop();

		Console.WriteLine("Последовательное выполнение завершено.");
		Console.WriteLine($"Итоговая сумма: {sum}");
		Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
	}

	// =========================
	// CPU-bound метод
	// =========================
	static int HeavyCalculation(int x)
	{
		// Локальная переменная — не разделяется между потоками
		int result = 0;

		// Искусственно нагружаем процессор
		// Это важно, чтобы параллельность имела смысл на следующих шагах
		for (int i = 0; i < 100; i++)
		{
			result += (x * i) % 7;
		}

		return result;
	}
}
