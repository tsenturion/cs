using System;
using System.Threading;

class Program
{
	static void Main()
	{
		Console.WriteLine("=== Демонстрация приоритетов потоков ===\n");

		// Создаем два потока
		Thread lowPriorityThread = new Thread(Work);
		Thread highPriorityThread = new Thread(Work);

		// Устанавливаем приоритеты до запуска
		lowPriorityThread.Priority = ThreadPriority.Lowest;
		highPriorityThread.Priority = ThreadPriority.Highest;

		Console.WriteLine($"Поток {lowPriorityThread.ManagedThreadId}: Приоритет {lowPriorityThread.Priority}");
		Console.WriteLine($"Поток {highPriorityThread.ManagedThreadId}: Приоритет {highPriorityThread.Priority}");
		Console.WriteLine();

		// Запускаем потоки
		lowPriorityThread.Start();
		highPriorityThread.Start();

		// Ждем завершения обоих потоков
		lowPriorityThread.Join();
		highPriorityThread.Join();

		Console.WriteLine("\nОба потока завершили работу");

		// Демонстрация с сохранением информации о приоритетах
		DemonstratePriorityEffects();
	}

	static void Work()
	{
		int threadId = Thread.CurrentThread.ManagedThreadId;
		ThreadPriority priority = Thread.CurrentThread.Priority;

		Console.WriteLine($"Поток {threadId} ({priority}) запущен");

		long counter = 0;
		DateTime start = DateTime.Now;

		// Делаем вычисления
		for (int i = 0; i < 1000000; i++)
		{
			counter++;
			if (i % 100000 == 0)
			{
				Console.WriteLine($"Поток {threadId} ({priority}): шаг {i / 100000 + 1}/10");
			}
		}

		TimeSpan duration = DateTime.Now - start;
		Console.WriteLine($"Поток {threadId} ({priority}) завершен за {duration.TotalMilliseconds:F0} мс");
	}

	static void DemonstratePriorityEffects()
	{
		Console.WriteLine("\n=== Влияние приоритетов на планирование ===");

		// Создаем потоки с разными приоритетами
		const int threadCount = 4;
		Thread[] threads = new Thread[threadCount];
		long[] counters = new long[threadCount];
		ThreadPriority[] priorities = new ThreadPriority[threadCount];

		// Сохраняем приоритеты перед запуском
		priorities[0] = ThreadPriority.Lowest;
		priorities[1] = ThreadPriority.BelowNormal;
		priorities[2] = ThreadPriority.AboveNormal;
		priorities[3] = ThreadPriority.Highest;

		// Создаем и настраиваем потоки
		for (int i = 0; i < threadCount; i++)
		{
			int threadIndex = i;
			threads[i] = new Thread(() =>
			{
				for (int j = 0; j < 5000000; j++)
				{
					counters[threadIndex]++;
				}
			});

			// Устанавливаем приоритет перед запуском
			threads[i].Priority = priorities[i];
		}

		Console.WriteLine("Запускаем 4 потока с разными приоритетами...");
		DateTime start = DateTime.Now;

		// Запускаем все потоки
		foreach (var thread in threads)
		{
			thread.Start();
		}

		// Ждем завершения всех потоков
		foreach (var thread in threads)
		{
			thread.Join();
		}

		TimeSpan totalDuration = DateTime.Now - start;

		// Используем сохраненные приоритеты
		Console.WriteLine("\nРезультаты:");
		for (int i = 0; i < threadCount; i++)
		{
			Console.WriteLine($"Поток {i} ({priorities[i]}): {counters[i]} итераций");
		}
		Console.WriteLine($"Общее время: {totalDuration.TotalMilliseconds:F0} мс");
	}
}