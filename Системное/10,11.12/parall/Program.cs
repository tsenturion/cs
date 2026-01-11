using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== volatile и модель памяти ===\n");

		// 1. Проблема видимости памяти
		Console.WriteLine("1. Проблема видимости памяти:");
		await VisibilityProblemDemo();

		// 2. volatile как решение
		Console.WriteLine("\n2. volatile как решение:");
		await VolatileSolutionDemo();

		// 3. volatile vs Interlocked
		Console.WriteLine("\n3. volatile vs Interlocked:");
		await VolatileVsInterlocked();

		// 4. Сравнение с lock
		Console.WriteLine("\n4. Сравнение с lock:");
		await CompareWithLock();

		// 5. Ограничения volatile
		Console.WriteLine("\n5. Ограничения volatile:");
		await VolatileLimitations();
	}

	// 1. Проблема видимости памяти
	static async Task VisibilityProblemDemo()
	{
		Console.WriteLine("Сценарий: поток-исполнитель и поток-сигнальщик");

		var worker = new NonVolatileWorker();
		Task workerTask = Task.Run(() => worker.DoWork());

		// Даем поработать
		await Task.Delay(100);

		Console.WriteLine("Сигнальщик: посылаю сигнал остановки");
		worker.RequestStop();

		// Ждем завершения
		bool completed = await Task.Run(() => workerTask.Wait(2000));

		if (completed)
		{
			Console.WriteLine("Рабочий поток завершился");
		}
		else
		{
			Console.WriteLine("Рабочий поток не завершился (проблема видимости!)");
			worker.ForceStop();
		}

		Console.WriteLine($"Итераций выполнено: {worker.Iterations}");
	}

	// 2. volatile как решение
	static async Task VolatileSolutionDemo()
	{
		Console.WriteLine("Тот же сценарий с volatile");

		var worker = new VolatileWorker();
		Task workerTask = Task.Run(() => worker.DoWork());

		await Task.Delay(100);

		Console.WriteLine("Сигнальщик: посылаю сигнал остановки");
		worker.RequestStop();

		bool completed = await Task.Run(() => workerTask.Wait(1000));

		if (completed)
		{
			Console.WriteLine("Рабочий поток завершился корректно");
		}
		else
		{
			Console.WriteLine("Рабочий поток не завершился");
			worker.ForceStop();
		}

		Console.WriteLine($"Итераций выполнено: {worker.Iterations}");
	}

	// 3. volatile vs Interlocked
	static async Task VolatileVsInterlocked()
	{
		Console.WriteLine("Тест 1: volatile с инкрементом (небезопасно)");

		var unsafeCounter = new VolatileCounter();
		Task[] unsafeTasks = new Task[10];

		for (int i = 0; i < 10; i++)
		{
			unsafeTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < 1000; j++)
				{
					unsafeCounter.Increment();
				}
			});
		}

		await Task.WhenAll(unsafeTasks);
		Console.WriteLine($"Ожидаемое: 10000, Получено: {unsafeCounter.Value}");
		Console.WriteLine("Результат меньше ожидаемого из-за гонки данных");

		Console.WriteLine("\nТест 2: Interlocked (атомарно)");

		var safeCounter = new InterlockedCounter();
		Task[] safeTasks = new Task[10];

		for (int i = 0; i < 10; i++)
		{
			safeTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < 1000; j++)
				{
					safeCounter.Increment();
				}
			});
		}

		await Task.WhenAll(safeTasks);
		Console.WriteLine($"Ожидаемое: 10000, Получено: {safeCounter.Value}");
		Console.WriteLine("Атомарные операции предотвращают гонки");
	}

	// 4. Сравнение с lock
	static async Task CompareWithLock()
	{
		const int iterations = 10000;
		const int threadCount = 4;

		Console.WriteLine($"Тест: {iterations} итераций, {threadCount} потока");

		// Тест с volatile
		var volatileCounter = new VolatileCounter();
		var volatileStopwatch = System.Diagnostics.Stopwatch.StartNew();

		Task[] volatileTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			volatileTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations / threadCount; j++)
				{
					volatileCounter.Increment();
				}
			});
		}

		await Task.WhenAll(volatileTasks);
		volatileStopwatch.Stop();

		// Тест с lock
		var lockCounter = new LockCounter();
		var lockStopwatch = System.Diagnostics.Stopwatch.StartNew();

		Task[] lockTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			lockTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations / threadCount; j++)
				{
					lockCounter.Increment();
				}
			});
		}

		await Task.WhenAll(lockTasks);
		lockStopwatch.Stop();

		// Тест с Interlocked
		var interlockedCounter = new InterlockedCounter();
		var interlockedStopwatch = System.Diagnostics.Stopwatch.StartNew();

		Task[] interlockedTasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			interlockedTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations / threadCount; j++)
				{
					interlockedCounter.Increment();
				}
			});
		}

		await Task.WhenAll(interlockedTasks);
		interlockedStopwatch.Stop();

		Console.WriteLine($"volatile: {volatileStopwatch.ElapsedMilliseconds} мс, результат: {volatileCounter.Value}");
		Console.WriteLine($"lock: {lockStopwatch.ElapsedMilliseconds} мс, результат: {lockCounter.Value}");
		Console.WriteLine($"Interlocked: {interlockedStopwatch.ElapsedMilliseconds} мс, результат: {interlockedCounter.Value}");

		Console.WriteLine("\nВыводы:");
		Console.WriteLine("- volatile быстрее, но небезопасен для сложных операций");
		Console.WriteLine("- lock безопасен, но медленнее");
		Console.WriteLine("- Interlocked оптимален для атомарных операций");
	}

	// 5. Ограничения volatile
	static async Task VolatileLimitations()
	{
		Console.WriteLine("Демонстрация ограничений volatile:");

		Console.WriteLine("\n1. volatile не обеспечивает атомарность");
		Console.WriteLine("   Инкремент требует Interlocked");

		Console.WriteLine("\n2. volatile не гарантирует согласованность нескольких полей");

		var holder = new MultiFieldHolder();

		Task writer = Task.Run(() =>
		{
			holder.SetValues(100, 200);
		});

		Task reader = Task.Run(() =>
		{
			var (a, b) = holder.GetValues();
			Console.WriteLine($"   Читатель: a={a}, b={b}");
			Console.WriteLine("   Может быть: a=100, b=0 (несогласованное состояние)");
		});

		await Task.WhenAll(writer, reader);

		Console.WriteLine("\n3. volatile не работает с локальными переменными");
		Console.WriteLine("   Только с полями класса");

		Console.WriteLine("\n4. Правильный сценарий для volatile - флаги состояния");

		var service = new VolatileService();
		Task serviceTask = Task.Run(() => service.Run());

		await Task.Delay(100);
		Console.WriteLine("   Останавливаю сервис...");
		service.Stop();

		await Task.Delay(100);
		Console.WriteLine("   Сервис корректно остановлен");
	}
}

// 1. Классы для демонстрации проблемы видимости
class NonVolatileWorker
{
	private bool _stopRequested = false;
	private long _iterations = 0;

	public long Iterations => _iterations;

	public void DoWork()
	{
		while (!_stopRequested)
		{
			_iterations++;
			Thread.Sleep(1);
		}
	}

	public void RequestStop()
	{
		_stopRequested = true;
	}

	public void ForceStop()
	{
		// Ничего не делаем, просто для завершения задачи
	}
}

class VolatileWorker
{
	private volatile bool _stopRequested = false;
	private long _iterations = 0;

	public long Iterations => _iterations;

	public void DoWork()
	{
		while (!_stopRequested)
		{
			_iterations++;
			Thread.Sleep(1);
		}
	}

	public void RequestStop()
	{
		_stopRequested = true;
	}

	public void ForceStop()
	{
		// Ничего не делаем
	}
}

// 2. Классы для сравнения подходов
class VolatileCounter
{
	private volatile int _value = 0;

	public int Value => _value;

	public void Increment()
	{
		_value++; // НЕ атомарно, даже с volatile!
	}
}

class InterlockedCounter
{
	private int _value = 0;

	public int Value => _value;

	public void Increment()
	{
		Interlocked.Increment(ref _value);
	}
}

class LockCounter
{
	private int _value = 0;
	private readonly object _lock = new object();

	public int Value => _value;

	public void Increment()
	{
		lock (_lock)
		{
			_value++;
		}
	}
}

// 3. Класс для демонстрации несогласованности
class MultiFieldHolder
{
	public volatile int FieldA = 0;
	public volatile int FieldB = 0;

	public void SetValues(int a, int b)
	{
		FieldA = a;
		// Между присваиваниями может вклиниться другой поток
		FieldB = b;
	}

	public (int, int) GetValues()
	{
		return (FieldA, FieldB);
	}
}

// 4. Правильный сценарий использования volatile
class VolatileService
{
	private volatile bool _isRunning = true;

	public void Run()
	{
		int count = 0;
		while (_isRunning)
		{
			count++;
			Thread.Sleep(10);
		}
		Console.WriteLine($"   Сервис выполнил {count} итераций");
	}

	public void Stop()
	{
		_isRunning = false;
	}
}