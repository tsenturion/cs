using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static void Main()
	{
		// =====================================================
		// ШАГ 1. Подготовка данных
		// =====================================================

		List<int> numbers = new List<int>(10_000_000);

		for (int i = 1; i <= 10_000_000; i++)
		{
			numbers.Add(i);
		}

		Console.WriteLine($"Количество элементов: {numbers.Count}");
		Console.WriteLine("Шаг 1 завершён: данные подготовлены.");
		Console.WriteLine();

		// =====================================================
		// ШАГ 2. Последовательное решение
		// =====================================================

		Stopwatch stopwatch = Stopwatch.StartNew();

		long sequentialSum = 0;

		foreach (int number in numbers)
		{
			sequentialSum += HeavyCalculation(number);
		}

		stopwatch.Stop();

		Console.WriteLine("ШАГ 2. Последовательное выполнение");
		Console.WriteLine($"Сумма: {sequentialSum}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// =====================================================
		// ШАГ 3. Неправильный параллельный вариант
		// =====================================================

		stopwatch.Restart();

		long parallelSumWrong = 0;

		Parallel.ForEach(numbers, number =>
		{
			// НАМЕРЕННАЯ ОШИБКА:
			// Состояние гонки при изменении общей переменной
			parallelSumWrong += HeavyCalculation(number);
		});

		stopwatch.Stop();

		Console.WriteLine("ШАГ 3. Параллельный вариант (НЕПРАВИЛЬНЫЙ)");
		Console.WriteLine($"Сумма: {parallelSumWrong}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// =====================================================
		// ШАГ 4. Корректный Parallel.ForEach
		// =====================================================

		stopwatch.Restart();

		long parallelSumCorrect = 0;

		Parallel.ForEach(
			numbers,
			() => 0L,
			(number, state, localSum) =>
			{
				return localSum + HeavyCalculation(number);
			},
			localSum =>
			{
				Interlocked.Add(ref parallelSumCorrect, localSum);
			});

		stopwatch.Stop();

		Console.WriteLine("ШАГ 4. Параллельный вариант (КОРРЕКТНЫЙ)");
		Console.WriteLine($"Сумма: {parallelSumCorrect}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// =====================================================
		// ШАГ 5. Решение с использованием PLINQ
		// =====================================================

		stopwatch.Restart();

		long plinqSum = numbers
			.AsParallel()
			.Select(HeavyCalculation)
			.Sum(x => (long)x);

		stopwatch.Stop();

		Console.WriteLine("ШАГ 5. PLINQ (без сохранения порядка)");
		Console.WriteLine($"Сумма: {plinqSum}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// --- PLINQ с сохранением порядка ---

		stopwatch.Restart();

		long plinqOrderedSum = numbers
			.AsParallel()
			.AsOrdered()
			.Select(HeavyCalculation)
			.Sum(x => (long)x);

		stopwatch.Stop();

		Console.WriteLine("ШАГ 5. PLINQ (С СОХРАНЕНИЕМ ПОРЯДКА)");
		Console.WriteLine($"Сумма: {plinqOrderedSum}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// --- PLINQ с ограничением параллелизма ---

		stopwatch.Restart();

		long plinqLimitedSum = numbers
			.AsParallel()
			.WithDegreeOfParallelism(4)
			.Select(HeavyCalculation)
			.Sum(x => (long)x);

		stopwatch.Stop();

		Console.WriteLine("ШАГ 5. PLINQ (ограничение параллелизма = 4)");
		Console.WriteLine($"Сумма: {plinqLimitedSum}");
		Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine();

		// =====================================================
		// ШАГ 6. Итоговый анализ результатов
		// =====================================================

		Console.WriteLine("ШАГ 6. СРАВНЕНИЕ РЕЗУЛЬТАТОВ");
		Console.WriteLine($"Последовательный:           {sequentialSum}");
		Console.WriteLine($"Параллельный (ошибка):      {parallelSumWrong}");
		Console.WriteLine($"Parallel.ForEach (правил.): {parallelSumCorrect}");
		Console.WriteLine($"PLINQ:                      {plinqSum}");
		Console.WriteLine($"PLINQ (ordered):            {plinqOrderedSum}");
		Console.WriteLine($"PLINQ (limit = 4):          {plinqLimitedSum}");
	}

	// =====================================================
	// CPU-bound метод
	// =====================================================
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
