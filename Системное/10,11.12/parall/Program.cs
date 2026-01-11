using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
	static void Main()
	{
		// =========================
		// ШАГ 1. Подготовка данных
		// =========================

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

		Stopwatch stopwatch = Stopwatch.StartNew();

		long sequentialSum = 0;

		foreach (int number in numbers)
		{
			sequentialSum += HeavyCalculation(number);
		}

		stopwatch.Stop();

		Console.WriteLine("ШАГ 2. Последовательное выполнение");
		Console.WriteLine($"Итоговая сумма: {sequentialSum}");
		Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// =========================
		// ШАГ 3. Неправильный параллельный вариант
		// =========================

		stopwatch.Restart();

		long parallelSumWrong = 0;

		Parallel.ForEach(numbers, number =>
		{
			// НАМЕРЕННАЯ ОШИБКА:
			// Несколько потоков одновременно изменяют одну переменную
			parallelSumWrong += HeavyCalculation(number);
		});

		stopwatch.Stop();

		Console.WriteLine("ШАГ 3. Параллельный вариант (НЕПРАВИЛЬНЫЙ)");
		Console.WriteLine($"Итоговая сумма: {parallelSumWrong}");
		Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// Сравнение результатов
		Console.WriteLine("Сравнение результатов:");
		Console.WriteLine($"Последовательный результат: {sequentialSum}");
		Console.WriteLine($"Параллельный результат:     {parallelSumWrong}");
	}

	// =========================
	// CPU-bound метод
	// =========================
	static int HeavyCalculation(int x)
	{
		int result = 0;

		for (int i = 0; i < 100; i++)
		{
			result += (x * i) % 7;
		}

		return result;
	}
}
