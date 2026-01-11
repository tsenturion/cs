using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Параллельные циклы и PLINQ ===\n");

		// 1. Parallel.For - основы
		Console.WriteLine("1. Parallel.For - основы:");
		await ParallelForBasics();

		// 2. Parallel.ForEach
		Console.WriteLine("\n2. Parallel.ForEach:");
		await ParallelForEachExample();

		// 3. PLINQ - основы
		Console.WriteLine("\n3. PLINQ - основы:");
		await PlinqBasics();

		// 4. Сравнение производительности
		Console.WriteLine("\n4. Сравнение производительности:");
		await PerformanceComparison();

		// 5. Ошибки и исключения
		Console.WriteLine("\n5. Ошибки и исключения:");
		await ExceptionsHandling();

		// 6. Ограничения параллелизма
		Console.WriteLine("\n6. Ограничения параллелизма:");
		await ParallelismLimits();
	}

	// 1. Parallel.For - основы
	static async Task ParallelForBasics()
	{
		Console.WriteLine("Простой Parallel.For:");

		Parallel.For(0, 10, i =>
		{
			Console.Write($"{i} ");
		});

		Console.WriteLine("\n\nПорядок выполнения не гарантирован!");

		Console.WriteLine("\nАккумулятор с локальными значениями:");

		long total = 0;
		Parallel.For(
			0,
			1000,
			() => 0L,
			(i, state, localSum) =>
			{
				return localSum + i;
			},
			localSum =>
			{
				Interlocked.Add(ref total, localSum);
			});

		Console.WriteLine($"Сумма 0..999: {total}");

		Console.WriteLine("\nРучное прерывание:");

		var cts = new CancellationTokenSource();
		ParallelOptions options = new ParallelOptions
		{
			CancellationToken = cts.Token
		};

		try
		{
			Parallel.For(0, 100, options, (i, state) =>
			{
				if (i == 50)
				{
					state.Stop();
					Console.WriteLine($"Остановка на итерации {i}");
				}

				if (state.IsStopped)
				{
					return;
				}

				Console.Write($"{i} ");
			});
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Операция отменена");
		}
	}

	// 2. Parallel.ForEach
	static async Task ParallelForEachExample()
	{
		var items = Enumerable.Range(1, 20).ToList();

		Console.WriteLine("Parallel.ForEach с коллекцией:");

		var results = new ConcurrentBag<int>();

		Parallel.ForEach(items, item =>
		{
			int result = ProcessItem(item);
			results.Add(result);
			Console.Write($"{result} ");
		});

		Console.WriteLine($"\nОбработано элементов: {results.Count}");

		Console.WriteLine("\nParallel.ForEach с Partitioner:");

		var largeList = Enumerable.Range(1, 10000).ToList();
		var partitioner = Partitioner.Create(largeList, true);

		long partitionerSum = 0;

		Parallel.ForEach(partitioner, () => 0L, (item, state, localSum) =>
		{
			return localSum + item;
		}, localSum =>
		{
			Interlocked.Add(ref partitionerSum, localSum);
		});

		Console.WriteLine($"Сумма через Partitioner: {partitionerSum}");
	}

	// 3. PLINQ - основы
	static async Task PlinqBasics()
	{
		var numbers = Enumerable.Range(1, 20);

		Console.WriteLine("Простой PLINQ запрос:");

		var squared = numbers
			.AsParallel()
			.Select(x =>
			{
				Console.Write($"{x} ");
				return x * x;
			})
			.ToList();

		Console.WriteLine($"\nКвадраты: {string.Join(", ", squared.Take(10))}...");

		Console.WriteLine("\nPLINQ с сохранением порядка:");

		var orderedSquares = numbers
			.AsParallel()
			.AsOrdered()
			.Select(x => x * x)
			.ToList();

		Console.WriteLine($"Квадраты по порядку: {string.Join(", ", orderedSquares.Take(10))}...");

		Console.WriteLine("\nPLINQ с фильтрацией:");

		var evenSquares = numbers
			.AsParallel()
			.Where(x => x % 2 == 0)
			.Select(x => x * x)
			.ToList();

		Console.WriteLine($"Квадраты четных: {string.Join(", ", evenSquares.Take(5))}...");

		Console.WriteLine("\nPLINQ с агрегацией:");

		long parallelSum = numbers
			.AsParallel()
			.Sum(x => (long)x);

		Console.WriteLine($"Сумма через PLINQ: {parallelSum}");
	}

	// 4. Сравнение производительности
	static async Task PerformanceComparison()
	{
		const int size = 1000000;
		var data = Enumerable.Range(1, size).ToArray();

		Console.WriteLine($"Тест на {size:N0} элементах");

		// Последовательный LINQ
		var sequentialStopwatch = Stopwatch.StartNew();
		long seqSum = data.Sum(x => (long)x);
		sequentialStopwatch.Stop();

		// Parallel.For
		var parallelForStopwatch = Stopwatch.StartNew();
		long parallelForSum = 0;

		Parallel.For(0, data.Length, () => 0L, (i, state, localSum) =>
		{
			return localSum + data[i];
		}, localSum =>
		{
			Interlocked.Add(ref parallelForSum, localSum);
		});
		parallelForStopwatch.Stop();

		// PLINQ
		var plinqStopwatch = Stopwatch.StartNew();
		long plinqSum = data.AsParallel().Sum(x => (long)x);
		plinqStopwatch.Stop();

		Console.WriteLine($"Последовательный LINQ: {sequentialStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"Parallel.For: {parallelForStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"PLINQ: {plinqStopwatch.ElapsedMilliseconds} мс");

		Console.WriteLine($"\nПроверка сумм: {seqSum == parallelForSum && seqSum == plinqSum}");

		Console.WriteLine("\nТест с тяжелыми вычислениями:");

		var computeStopwatch = Stopwatch.StartNew();
		var seqCompute = data.Select(HeavyComputation).ToList();
		computeStopwatch.Stop();

		var parallelComputeStopwatch = Stopwatch.StartNew();
		var parallelCompute = data.AsParallel().Select(HeavyComputation).ToList();
		parallelComputeStopwatch.Stop();

		Console.WriteLine($"Последовательно: {computeStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"Параллельно: {parallelComputeStopwatch.ElapsedMilliseconds} мс");

		double speedup = (double)computeStopwatch.ElapsedMilliseconds / parallelComputeStopwatch.ElapsedMilliseconds;
		Console.WriteLine($"Ускорение: {speedup:F1}x");
	}

	// 5. Ошибки и исключения
	static async Task ExceptionsHandling()
	{
		Console.WriteLine("Обработка исключений в Parallel.ForEach:");

		var items = Enumerable.Range(0, 10);
		var exceptions = new ConcurrentQueue<Exception>();

		try
		{
			Parallel.ForEach(items, i =>
			{
				try
				{
					if (i == 5)
					{
						throw new InvalidOperationException($"Ошибка в итерации {i}");
					}

					Console.Write($"{i} ");
				}
				catch (Exception ex)
				{
					exceptions.Enqueue(ex);
				}
			});

			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}
		}
		catch (AggregateException ex)
		{
			Console.WriteLine($"\nAggregateException: {ex.InnerExceptions.Count} исключений");
			foreach (var inner in ex.InnerExceptions)
			{
				Console.WriteLine($"  - {inner.Message}");
			}
		}

		Console.WriteLine("\nИсключения в PLINQ:");

		try
		{
			var result = items
				.AsParallel()
				.Select(i =>
				{
					if (i == 3)
					{
						throw new ArgumentException($"PLINQ ошибка в {i}");
					}
					return i * 2;
				})
				.ToList();
		}
		catch (AggregateException ex)
		{
			Console.WriteLine($"PLINQ AggregateException: {ex.InnerExceptions.Count} исключений");
		}
	}

	// 6. Ограничения параллелизма
	static async Task ParallelismLimits()
	{
		var numbers = Enumerable.Range(1, 100);

		Console.WriteLine("PLINQ с ограничением параллелизма:");

		var limitedResult = numbers
			.AsParallel()
			.WithDegreeOfParallelism(2)
			.Select(x =>
			{
				Console.Write($"{x} ");
				Thread.Sleep(10);
				return x;
			})
			.ToList();

		Console.WriteLine($"\nС DegreeOfParallelism = 2");

		Console.WriteLine("\nParallel.ForEach с ParallelOptions:");

		var options = new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		};

		Parallel.ForEach(numbers, options, x =>
		{
			Console.Write($"{x} ");
			Thread.Sleep(10);
		});

		Console.WriteLine($"\nС MaxDegreeOfParallelism = 3");

		Console.WriteLine("\nКогда НЕ использовать параллельные циклы:");

		// I/O-bound операции
		Console.WriteLine("I/O-bound операции (сеть, файлы) - лучше async/await");

		// Легкие вычисления
		var lightData = Enumerable.Range(1, 100);
		var lightStopwatch = Stopwatch.StartNew();
		var lightPlinq = lightData.AsParallel().Select(x => x + 1).ToList();
		lightStopwatch.Stop();

		var lightSeqStopwatch = Stopwatch.StartNew();
		var lightSeq = lightData.Select(x => x + 1).ToList();
		lightSeqStopwatch.Stop();

		Console.WriteLine($"Легкие вычисления (100 элементов):");
		Console.WriteLine($"  PLINQ: {lightStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"  LINQ: {lightSeqStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"  Накладные расходы: {(lightStopwatch.ElapsedMilliseconds - lightSeqStopwatch.ElapsedMilliseconds)} мс");
	}

	// Вспомогательные методы
	static int ProcessItem(int item)
	{
		Thread.Sleep(10);
		return item * 2;
	}

	static int HeavyComputation(int value)
	{
		// Имитация тяжелых вычислений
		long result = 0;
		for (int i = 0; i < 1000; i++)
		{
			result += (value + i) % 100;
		}
		return (int)result;
	}
}