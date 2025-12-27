using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Semaphore и SemaphoreSlim ===\n");

		// 1. Semaphore - межпроцессный
		Console.WriteLine("1. Semaphore (межпроцессный):");
		await SemaphoreExample();

		// 2. SemaphoreSlim - внутрипроцессный
		Console.WriteLine("\n2. SemaphoreSlim (внутрипроцессный):");
		await SemaphoreSlimExample();

		// 3. WaitAsync - асинхронное ожидание
		Console.WriteLine("\n3. SemaphoreSlim с WaitAsync:");
		await SemaphoreSlimAsyncExample();

		// 4. Ограничение параллелизма
		Console.WriteLine("\n4. Ограничение параллелизма:");
		await ParallelismLimitation();

		// 5. Ошибки при работе с семафорами
		Console.WriteLine("\n5. Типичные ошибки:");
		await CommonMistakes();
	}

	// 1. Semaphore - межпроцессный
	static async Task SemaphoreExample()
	{
		// Создаем семафор на 3 одновременных входа
		using (Semaphore semaphore = new Semaphore(3, 3, "Global\\MySemaphore"))
		{
			Console.WriteLine("Семафор создан. Максимум 3 одновременных входа");

			// Имитация нескольких "процессов"
			Task[] processes = new Task[5];

			for (int i = 1; i <= 5; i++)
			{
				int processId = i;
				processes[i - 1] = Task.Run(() => ProcessWithSemaphore(processId, semaphore));
				await Task.Delay(100);
			}

			await Task.WhenAll(processes);
			Console.WriteLine("Все процессы завершили работу");
		}
	}

	static void ProcessWithSemaphore(int id, Semaphore semaphore)
	{
		Console.WriteLine($"  Процесс {id}: жду входа");
		semaphore.WaitOne();

		try
		{
			Console.WriteLine($"  Процесс {id}: вошел");
			Thread.Sleep(1500);
		}
		finally
		{
			semaphore.Release();
			Console.WriteLine($"  Процесс {id}: вышел");
		}
	}

	// 2. SemaphoreSlim - внутрипроцессный
	static async Task SemaphoreSlimExample()
	{
		// Облегченный семафор на 2 одновременных входа
		using (SemaphoreSlim semaphore = new SemaphoreSlim(2))
		{
			Console.WriteLine($"Семафор создан. Доступно мест: {semaphore.CurrentCount}");

			Task[] tasks = new Task[4];

			for (int i = 1; i <= 4; i++)
			{
				int taskId = i;
				tasks[i - 1] = Task.Run(() => ProcessWithSemaphoreSlim(taskId, semaphore));
			}

			await Task.WhenAll(tasks);
			Console.WriteLine("Все задачи завершены");
		}
	}

	static void ProcessWithSemaphoreSlim(int id, SemaphoreSlim semaphore)
	{
		Console.WriteLine($"  Задача {id}: жду входа");
		semaphore.Wait();

		try
		{
			Console.WriteLine($"  Задача {id}: вошла");
			Console.WriteLine($"  Доступно мест: {semaphore.CurrentCount}");
			Thread.Sleep(1000);
		}
		finally
		{
			semaphore.Release();
			Console.WriteLine($"  Задача {id}: вышла");
		}
	}

	// 3. WaitAsync - асинхронное ожидание
	static async Task SemaphoreSlimAsyncExample()
	{
		using (SemaphoreSlim semaphore = new SemaphoreSlim(2))
		{
			Console.WriteLine("Запускаем асинхронные задачи...");

			Task[] tasks = new Task[5];

			for (int i = 1; i <= 5; i++)
			{
				int taskId = i;
				tasks[i - 1] = WorkerAsync(taskId, semaphore);
				await Task.Delay(50);
			}

			await Task.WhenAll(tasks);
			Console.WriteLine("Все асинхронные задачи завершены");
		}
	}

	static async Task WorkerAsync(int id, SemaphoreSlim semaphore)
	{
		Console.WriteLine($"  Задача {id}: начинаю ожидание");
		await semaphore.WaitAsync();

		try
		{
			Console.WriteLine($"  Задача {id}: вошла в критическую секцию");
			await Task.Delay(800); // Асинхронная работа
			Console.WriteLine($"  Задача {id}: завершила работу");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// 4. Ограничение параллелизма
	static async Task ParallelismLimitation()
	{
		Console.WriteLine("Ограничение запросов к внешнему API...");

		// Максимум 3 одновременных запроса к API
		using (SemaphoreSlim apiThrottler = new SemaphoreSlim(3))
		{
			Task[] apiCalls = new Task[10];

			for (int i = 1; i <= 10; i++)
			{
				int requestId = i;
				apiCalls[i - 1] = MakeApiCallAsync(requestId, apiThrottler);
			}

			await Task.WhenAll(apiCalls);
			Console.WriteLine("Все запросы к API выполнены");
		}
	}

	static async Task MakeApiCallAsync(int id, SemaphoreSlim throttler)
	{
		Console.WriteLine($"  Запрос {id}: в очереди");
		await throttler.WaitAsync();

		try
		{
			Console.WriteLine($"  Запрос {id}: выполняется");
			await Task.Delay(300); // Имитация API-вызова
			Console.WriteLine($"  Запрос {id}: завершен");
		}
		finally
		{
			throttler.Release();
		}
	}

	// 5. Ошибки при работе с семафорами
	static async Task CommonMistakes()
	{
		Console.WriteLine("Демонстрация ошибок:");

		// Ошибка 1: Забытый Release
		Console.WriteLine("\nОшибка 1: Забытый Release");
		SemaphoreSlim badSemaphore = new SemaphoreSlim(1);
		try
		{
			Console.WriteLine($"  Доступно мест: {badSemaphore.CurrentCount}");

			badSemaphore.Wait();
			Console.WriteLine($"  После Wait: {badSemaphore.CurrentCount}");

			// Забыли Release!
			// Пытаемся захватить еще раз (дедлок)
			bool acquired = badSemaphore.Wait(500);
			Console.WriteLine($"  Второй Wait: {acquired}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Исключение: {ex.GetType().Name}");
		}
		finally
		{
			badSemaphore.Dispose();
		}

		// Ошибка 2: Слишком много Release
		Console.WriteLine("\nОшибка 2: Слишком много Release");
		try
		{
			SemaphoreSlim overflowSemaphore = new SemaphoreSlim(2, 2);

			Console.WriteLine($"  Изначально доступно: {overflowSemaphore.CurrentCount}");
			overflowSemaphore.Wait();
			Console.WriteLine($"  После Wait: {overflowSemaphore.CurrentCount}");

			overflowSemaphore.Release();
			Console.WriteLine($"  После Release: {overflowSemaphore.CurrentCount}");

			overflowSemaphore.Release(); // Второй Release - нормально
			Console.WriteLine($"  После второго Release: {overflowSemaphore.CurrentCount}");

			// Третий Release будет вызывать исключение
			overflowSemaphore.Release();
		}
		catch (SemaphoreFullException ex)
		{
			Console.WriteLine($"  SemaphoreFullException: {ex.Message}");
		}

		// Ошибка 3: Использование как lock
		Console.WriteLine("\nОшибка 3: Семафор вместо lock");
		try
		{
			int counter = 0;
			SemaphoreSlim wrongLock = new SemaphoreSlim(1);

			Task[] tasks = new Task[10];
			for (int i = 0; i < 10; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					wrongLock.Wait();
					try
					{
						counter++;
					}
					finally
					{
						wrongLock.Release();
					}
				});
			}

			await Task.WhenAll(tasks);
			Console.WriteLine($"  Результат: {counter}");
			Console.WriteLine("  Работает, но это неоправданно сложно");
			wrongLock.Dispose();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Ошибка: {ex.Message}");
		}

		// Правильный подход с lock
		Console.WriteLine("\nПравильный подход (lock):");
		int correctCounter = 0;
		object lockObj = new object();

		Task[] correctTasks = new Task[10];
		for (int i = 0; i < 10; i++)
		{
			correctTasks[i] = Task.Run(() =>
			{
				lock (lockObj)
				{
					correctCounter++;
				}
			});
		}

		await Task.WhenAll(correctTasks);
		Console.WriteLine($"  Результат: {correctCounter}");
		Console.WriteLine("  Проще и быстрее");
	}
}