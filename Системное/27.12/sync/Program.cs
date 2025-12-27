using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== ReaderWriterLockSlim ===\n");

		// 1. Базовое использование
		Console.WriteLine("1. Базовое использование:");
		await BasicExample();

		// 2. Сравнение с lock
		Console.WriteLine("\n2. Сравнение с lock:");
		await CompareWithLock();

		// 3. UpgradeableReadLock
		Console.WriteLine("\n3. UpgradeableReadLock:");
		await UpgradeableReadExample();

		// 4. Много читателей, мало писателей
		Console.WriteLine("\n4. Много читателей, мало писателей:");
		await ManyReadersFewWriters();

		// 5. Типичные ошибки
		Console.WriteLine("\n5. Типичные ошибки:");
		await CommonMistakes();
	}

	// 1. Базовое использование
	static async Task BasicExample()
	{
		var cache = new SharedCache();

		Console.WriteLine("Запускаем читателей...");

		Task[] readers = new Task[3];
		for (int i = 0; i < 3; i++)
		{
			int readerId = i + 1;
			readers[i] = Task.Run(() =>
			{
				int value = cache.Read();
				Console.WriteLine($"  Читатель {readerId}: {value}");
			});
		}

		await Task.WhenAll(readers);

		Console.WriteLine("Писатель обновляет данные...");
		cache.Write(42);

		Console.WriteLine("Проверяем обновленные данные...");
		for (int i = 0; i < 2; i++)
		{
			int readerId = i + 4;
			Task.Run(() =>
			{
				int value = cache.Read();
				Console.WriteLine($"  Читатель {readerId}: {value}");
			});
		}

		await Task.Delay(500);
	}

	// 2. Сравнение с lock
	static async Task CompareWithLock()
	{
		const int readerCount = 1000;
		const int iterations = 10000;

		Console.WriteLine($"Тест: {readerCount} читателей, {iterations} итераций");

		// Тест с ReaderWriterLockSlim
		var rwLock = new ReaderWriterLockSlim();
		int rwCounter = 0;
		var rwStopwatch = System.Diagnostics.Stopwatch.StartNew();

		Task[] rwTasks = new Task[readerCount];
		for (int i = 0; i < readerCount; i++)
		{
			rwTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations; j++)
				{
					rwLock.EnterReadLock();
					try
					{
						_ = rwCounter; // Чтение
					}
					finally
					{
						rwLock.ExitReadLock();
					}
				}
			});
		}

		await Task.WhenAll(rwTasks);
		rwStopwatch.Stop();

		// Тест с lock
		object lockObj = new object();
		int lockCounter = 0;
		var lockStopwatch = System.Diagnostics.Stopwatch.StartNew();

		Task[] lockTasks = new Task[readerCount];
		for (int i = 0; i < readerCount; i++)
		{
			lockTasks[i] = Task.Run(() =>
			{
				for (int j = 0; j < iterations; j++)
				{
					lock (lockObj)
					{
						_ = lockCounter; // Чтение
					}
				}
			});
		}

		await Task.WhenAll(lockTasks);
		lockStopwatch.Stop();

		Console.WriteLine($"ReaderWriterLockSlim: {rwStopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"lock: {lockStopwatch.ElapsedMilliseconds} мс");

		if (lockStopwatch.ElapsedMilliseconds > 0)
		{
			double ratio = (double)lockStopwatch.ElapsedMilliseconds / rwStopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Выигрыш: {ratio:F1}x");
		}
	}

	// 3. UpgradeableReadLock
	static async Task UpgradeableReadExample()
	{
		var database = new DatabaseSimulator();

		Console.WriteLine("Запускаем операции с UpgradeableReadLock...");

		Task[] updaters = new Task[3];
		for (int i = 0; i < 3; i++)
		{
			int updaterId = i + 1;
			updaters[i] = Task.Run(() =>
			{
				// Поток читает, и при необходимости обновляет
				bool updated = database.UpdateIfNeeded(updaterId * 10);
				Console.WriteLine($"  Поток {updaterId}: {(updated ? "обновил" : "не изменил")}");
			});
		}

		// Читатели во время обновлений
		Task[] readers = new Task[2];
		for (int i = 0; i < 2; i++)
		{
			int readerId = i + 1;
			readers[i] = Task.Run(() =>
			{
				int value = database.Read();
				Console.WriteLine($"  Читатель {readerId}: {value}");
			});
		}

		await Task.WhenAll(updaters);
		await Task.WhenAll(readers);
	}

	// 4. Много читателей, мало писателей
	static async Task ManyReadersFewWriters()
	{
		var config = new Configuration();

		Console.WriteLine("Ситуация: 5 читателей, 2 писателя");
		Console.WriteLine("Читатели работают постоянно, писатели - редко");

		CancellationTokenSource cts = new CancellationTokenSource();

		// Много читателей
		Task[] readers = new Task[5];
		for (int i = 0; i < 5; i++)
		{
			int readerId = i + 1;
			readers[i] = Task.Run(async () =>
			{
				while (!cts.Token.IsCancellationRequested)
				{
					string value = config.GetValue();
					Console.WriteLine($"  Читатель {readerId}: {value}");
					await Task.Delay(100);
				}
			});
		}

		// Мало писателей
		Task[] writers = new Task[2];
		for (int i = 0; i < 2; i++)
		{
			int writerId = i + 1;
			writers[i] = Task.Run(async () =>
			{
				for (int j = 0; j < 2; j++)
				{
					await Task.Delay(500);
					config.SetValue($"Значение от писателя {writerId}");
					Console.WriteLine($"  Писатель {writerId}: обновил");
				}
			});
		}

		await Task.WhenAll(writers);
		cts.CancelAfter(100);

		try
		{
			await Task.WhenAll(readers);
		}
		catch (OperationCanceledException) { }

		Console.WriteLine("Симуляция завершена");
	}

	// 5. Типичные ошибки
	static async Task CommonMistakes()
	{
		Console.WriteLine("Ошибка 1: Неправильный порядок блокировок");

		var rwLock = new ReaderWriterLockSlim();

		try
		{
			rwLock.EnterReadLock();
			Console.WriteLine("  Вошел в read lock");

			// Нельзя напрямую войти в write lock из read lock
			rwLock.EnterWriteLock(); // Исключение или дедлок
		}
		catch (LockRecursionException ex)
		{
			Console.WriteLine($"  LockRecursionException: {ex.Message}");
			rwLock.ExitReadLock();
		}

		Console.WriteLine("\nОшибка 2: Забытый Exit");

		var rwLock2 = new ReaderWriterLockSlim();
		try
		{
			rwLock2.EnterReadLock();
			Console.WriteLine("  Вошел в read lock");
			// Забыли ExitReadLock!
		}
		finally
		{
			// В реальном коде нужно в finally
			// rwLock2.ExitReadLock();
		}

		Console.WriteLine("  (в реальном коде это привело бы к проблемам)");

		Console.WriteLine("\nПравильный подход:");

		var rwLock3 = new ReaderWriterLockSlim();
		try
		{
			rwLock3.EnterReadLock();
			Console.WriteLine("  Вошел в read lock");
			// Работа с данными
		}
		finally
		{
			rwLock3.ExitReadLock();
			Console.WriteLine("  Вышел из read lock");
		}
	}
}

// 1. Простой кэш с ReaderWriterLockSlim
class SharedCache
{
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private int _data = 0;

	public int Read()
	{
		_lock.EnterReadLock();
		try
		{
			Thread.Sleep(50); // Имитация чтения
			return _data;
		}
		finally
		{
			_lock.ExitReadLock();
		}
	}

	public void Write(int value)
	{
		_lock.EnterWriteLock();
		try
		{
			Thread.Sleep(100); // Имитация записи
			_data = value;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}
}

// 2. База данных с UpgradeableReadLock
class DatabaseSimulator
{
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private int _value = 0;

	public int Read()
	{
		_lock.EnterReadLock();
		try
		{
			Thread.Sleep(30);
			return _value;
		}
		finally
		{
			_lock.ExitReadLock();
		}
	}

	public bool UpdateIfNeeded(int newValue)
	{
		_lock.EnterUpgradeableReadLock();
		try
		{
			if (_value == newValue)
			{
				return false; // Не нужно обновлять
			}

			_lock.EnterWriteLock();
			try
			{
				Thread.Sleep(50);
				_value = newValue;
				return true;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		finally
		{
			_lock.ExitUpgradeableReadLock();
		}
	}
}

// 3. Конфигурация с частым чтением
class Configuration
{
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private string _value = "default";

	public string GetValue()
	{
		_lock.EnterReadLock();
		try
		{
			return _value;
		}
		finally
		{
			_lock.ExitReadLock();
		}
	}

	public void SetValue(string newValue)
	{
		_lock.EnterWriteLock();
		try
		{
			_value = newValue;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}
}