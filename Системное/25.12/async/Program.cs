using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Демонстрация асинхронности vs многопоточности ===\n");

		// Пример 1: Чистая асинхронность (без создания потоков)
		Console.WriteLine("1. Чистая асинхронность (Task.Delay):");
		await DemonstrateAsyncAwait();

		// Пример 2: Проверка потоков в асинхронном коде
		Console.WriteLine("\n2. Анализ потоков в асинхронном коде:");
		await AnalyzeThreads();

		// Пример 3: Многопоточность (явное создание потока)
		Console.WriteLine("\n3. Многопоточность (явное создание Thread):");
		DemonstrateMultiThreading();

		// Пример 4: Неправильное использование Task.Run для I/O
		Console.WriteLine("\n4. Сравнение подходов для I/O операции:");
		await CompareIoApproaches();

		// Пример 5: CPU-bound задача
		Console.WriteLine("\n5. CPU-bound задача (вычисления):");
		await DemonstrateCpuBound();

		Console.WriteLine("\n=== Итог ===");
		Console.WriteLine("Асинхронность: эффективное ожидание");
		Console.WriteLine("Многопоточность: параллельные вычисления");
	}

	static async Task DemonstrateAsyncAwait()
	{
		Console.WriteLine("  До await. Поток: " + Thread.CurrentThread.ManagedThreadId);

		// Асинхронная задержка - поток освобождается
		await Task.Delay(1000);

		Console.WriteLine("  После await. Поток: " + Thread.CurrentThread.ManagedThreadId);
		Console.WriteLine("  Возможно тот же поток, возможно другой");
	}

	static async Task AnalyzeThreads()
	{
		int initialThreadId = Thread.CurrentThread.ManagedThreadId;
		Console.WriteLine($"  Начальный поток: {initialThreadId}");

		// Создаем несколько асинхронных операций
		Task[] tasks = new Task[3];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Task.Delay(500 + i * 100);
		}

		Console.WriteLine("  Запущено 3 Task.Delay");
		Console.WriteLine("  Основной поток свободен для другой работы");

		await Task.WhenAll(tasks);

		int finalThreadId = Thread.CurrentThread.ManagedThreadId;
		Console.WriteLine($"  Финальный поток: {finalThreadId}");
		Console.WriteLine($"  Совпадают: {initialThreadId == finalThreadId}");
	}

	static void DemonstrateMultiThreading()
	{
		Console.WriteLine("  Главный поток: " + Thread.CurrentThread.ManagedThreadId);

		// Явное создание потока (дорогая операция)
		Thread thread = new Thread(() =>
		{
			Console.WriteLine("  Рабочий поток: " + Thread.CurrentThread.ManagedThreadId);
			Thread.Sleep(500); // Блокировка потока
		});

		thread.Start();
		thread.Join();

		Console.WriteLine("  Создан отдельный поток с полным стеком");
	}

	static async Task CompareIoApproaches()
	{
		// Имитация I/O операции (чтение файла/сети)
		Console.WriteLine("  Имитация I/O операции...");

		// Неправильно: Task.Run для I/O
		var start = DateTime.Now;
		await Task.Run(() =>
		{
			Thread.Sleep(1000); // Имитация I/O ожидания
		});
		var time1 = DateTime.Now - start;

		// Правильно: асинхронное ожидание
		start = DateTime.Now;
		await Task.Delay(1000); // Аналогично асинхронному I/O
		var time2 = DateTime.Now - start;

		Console.WriteLine($"  Task.Run + Sleep: {time1.TotalMilliseconds:F0} мс");
		Console.WriteLine($"  Task.Delay (async): {time2.TotalMilliseconds:F0} мс");
		Console.WriteLine("  Время одинаково, но подходы разные");
	}

	static async Task DemonstrateCpuBound()
	{
		Console.WriteLine("  Запускаю CPU-intensive задачу...");

		// Правильно: Task.Run для вычислений
		int result = await Task.Run(() =>
		{
			Console.WriteLine($"  Вычисления в потоке: {Thread.CurrentThread.ManagedThreadId}");

			// Имитация тяжелых вычислений
			long sum = 0;
			for (int i = 0; i < 10000000; i++)
			{
				sum += i;
			}

			return (int)(sum % 1000);
		});

		Console.WriteLine($"  Результат вычислений: {result}");
		Console.WriteLine("  Для CPU-bound задач Task.Run оправдан");
	}
}