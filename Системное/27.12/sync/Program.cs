using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== SpinLock и активное ожидание ===\n");

		// 1. Базовое использование
		Console.WriteLine("1. Базовое использование SpinLock:");
		await BasicSpinLockExample();

		// 2. Сравнение с lock
		Console.WriteLine("\n2. Сравнение SpinLock и lock:");
		await CompareSpinLockWithLock();

		// 3. Длительное ожидание
		Console.WriteLine("\n3. Длительное ожидание (антипаттерн):");
		await LongWaitAntiPattern();

		// 4. Конкуренция потоков
		Console.WriteLine("\n4. Высокая конкуренция:");
		await HighContentionScenario();

		// 5. SpinWait
		Console.WriteLine("\n5. SpinWait - управляемое ожидание:");
		await SpinWaitExample();
	}

	// 1. Базовое использование
	static async Task BasicSpinLockExample()
	{
		SpinLock spinLock = new SpinLock();
		int counter = 0;

		Console.WriteLine("Запускаем короткие операции...");

		Task[] tasks = new Task[3];
		for (int i = 0; i < 3; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < 1000; j++)
				{
					bool lockTaken = false;
					try
					{
						spinLock.Enter(ref lockTaken);
						counter++; // Короткая операция
					}
					finally
					{
						if (lockTaken)
							spinLock.Exit();
					}
				}
			});
		}

		await Task.WhenAll(tasks);
		Console.WriteLine($"Результат: {counter}");
	}

	// 2. Сравнение с lock
	static async Task CompareSpinLockWithLock()
	{
		const int iterations = 100000;
		const int threadCount = 4;

		Console.WriteLine($"Тест: {iterations} итераций, {threadCount} потока");

		// Тест SpinLock
		SpinLock spinLock = new SpinLock();
		int spinLockCounter = 0;
		var spinLockStopwatch = Stopwatch.StartNew();

		Task[] spinLockTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			spinLockTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations / threadCount; j++)
				{
					bool lockTaken = false;
					try
					{
						spinLock.Enter(ref lockTaken);
						spinLockCounter++;
					}
					finally
					{
						if (lockTaken)
							spinLock.Exit();
					}
				}
			});
		}

		await Task.WhenAll(spinLockTasks);
		spinLockStopwatch.Stop();

		// Тест lock
		object lockObj = new object();
		int lockCounter = 0;
		var lockStopwatch = Stopwatch.StartNew();

		Task[] lockTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			lockTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations / threadCount; j++)
				{
					lock (lockObj)
					{
						lockCounter++;
					}
				}
			});
		}

		await Task.WhenAll(lockTasks);
		lockStopwatch.Stop();

		Console.WriteLine($"SpinLock: {spinLockStopwatch.ElapsedMilliseconds} мс, результат: {spinLockCounter}");
		Console.WriteLine($"lock: {lockStopwatch.ElapsedMilliseconds} мс, результат: {lockCounter}");

		if (lockStopwatch.ElapsedMilliseconds > 0 && spinLockStopwatch.ElapsedMilliseconds > 0)
		{
			double ratio = (double)lockStopwatch.ElapsedMilliseconds / spinLockStopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Отношение: {ratio:F2}x ({(ratio > 1 ? "SpinLock быстрее" : "lock быстрее")})");
		}
	}

	// 3. Длительное ожидание
	static async Task LongWaitAntiPattern()
	{
		Console.WriteLine("Демонстрация антипаттерна...");

		SpinLock spinLock = new SpinLock();
		bool resourceAvailable = false;

		// Поток, который долго держит ресурс
		Task longHolder = Task.Run(() =>
		{
			bool lockTaken = false;
			spinLock.Enter(ref lockTaken);

			try
			{
				Console.WriteLine("Держатель: захватил ресурс на 2 секунды");
				Thread.Sleep(2000);
			}
			finally
			{
				if (lockTaken)
				{
					spinLock.Exit();
					Console.WriteLine("Держатель: освободил ресурс");
				}
			}
		});

		await Task.Delay(100);

		// Поток с активным ожиданием
		Task spinner = Task.Run(() =>
		{
			Console.WriteLine("Ожидающий: начинаю активное ожидание");
			var waitStopwatch = Stopwatch.StartNew();

			bool lockTaken = false;
			while (!lockTaken)
			{
				spinLock.Enter(ref lockTaken);
				if (!lockTaken)
				{
					// Краткая пауза между попытками
					Thread.Sleep(1);
				}
			}

			waitStopwatch.Stop();

			try
			{
				Console.WriteLine($"Ожидающий: захватил ресурс через {waitStopwatch.ElapsedMilliseconds} мс");
				Console.WriteLine("Активное ожидание потребляет процессорное время!");
			}
			finally
			{
				spinLock.Exit();
			}
		});

		await Task.WhenAll(longHolder, spinner);
		Console.WriteLine("Ситуация, когда SpinLock вреден");
	}

	// 4. Конкуренция потоков
	static async Task HighContentionScenario()
	{
		Console.WriteLine("Высокая конкуренция (много потоков)...");

		SpinLock spinLock = new SpinLock();
		int counter = 0;
		int threadCount = 8;

		Console.WriteLine($"Запускаем {threadCount} потоков с высокой конкуренцией");

		Task[] highContentionTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			highContentionTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < 10000; j++)
				{
					bool lockTaken = false;
					try
					{
						spinLock.Enter(ref lockTaken);
						counter++;
					}
					finally
					{
						if (lockTaken)
							spinLock.Exit();
					}
				}
			});
		}

		await Task.WhenAll(highContentionTasks);
		Console.WriteLine($"Результат: {counter}");
		Console.WriteLine("При высокой конкуренции SpinLock теряет эффективность");
	}

	// 5. SpinWait
	static async Task SpinWaitExample()
	{
		Console.WriteLine("Использование SpinWait...");

		bool condition = false;
		int attempts = 0;

		Task setter = Task.Run(() =>
		{
			Thread.Sleep(500);
			condition = true;
			Console.WriteLine("Установил условие");
		});

		Task waiter = Task.Run(() =>
		{
			Console.WriteLine("Начинаю ожидание с SpinWait");
			var spinWait = new SpinWait();

			while (!condition)
			{
				spinWait.SpinOnce();
				attempts++;

				if (attempts % 100 == 0)
				{
					Console.WriteLine($"  Попытка {attempts}");
				}
			}

			Console.WriteLine($"Условие выполнено после {attempts} попыток");
		});

		await Task.WhenAll(setter, waiter);

		Console.WriteLine("\nSpinWait постепенно увеличивает паузы:");
		var demoSpinWait = new SpinWait();
		for (int i = 0; i < 5; i++)
		{
			demoSpinWait.SpinOnce();
			Console.WriteLine($"  Итерация {i + 1}: счетчик = {demoSpinWait.Count}");
		}
	}
}