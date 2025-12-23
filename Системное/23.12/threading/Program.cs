using System;
using System.Threading;

class Program
{
	static int counter = 0; // Общая переменная

	static void Main()
	{
		// Демонстрация состояния гонки
		Console.WriteLine("Запуск потоков с состоянием гонки...");

		Thread t1 = new Thread(Increment);
		Thread t2 = new Thread(Increment);

		t1.Start();
		t2.Start();

		t1.Join();
		t2.Join();

		Console.WriteLine($"counter после гонки: {counter}");

		// Тест с синхронизацией
		TestWithInterlocked();
		TestWithLock();
	}

	static void Increment()
	{
		for (int i = 0; i < 100000; i++)
		{
			counter++; // Небезопасно
		}
	}

	static void TestWithInterlocked()
	{
		Console.WriteLine("\n--- Тест с Interlocked ---");
		int safeCounter = 0;

		Thread t1 = new Thread(() =>
		{
			for (int i = 0; i < 100000; i++)
			{
				Interlocked.Increment(ref safeCounter);
			}
		});

		Thread t2 = new Thread(() =>
		{
			for (int i = 0; i < 100000; i++)
			{
				Interlocked.Increment(ref safeCounter);
			}
		});

		t1.Start();
		t2.Start();
		t1.Join();
		t2.Join();

		Console.WriteLine($"Результат: {safeCounter}");
	}

	static void TestWithLock()
	{
		Console.WriteLine("\n--- Тест с lock ---");
		int lockedCounter = 0;
		object lockObj = new object();

		Thread t1 = new Thread(() =>
		{
			for (int i = 0; i < 100000; i++)
			{
				lock (lockObj)
				{
					lockedCounter++;
				}
			}
		});

		Thread t2 = new Thread(() =>
		{
			for (int i = 0; i < 100000; i++)
			{
				lock (lockObj)
				{
					lockedCounter++;
				}
			}
		});

		t1.Start();
		t2.Start();
		t1.Join();
		t2.Join();

		Console.WriteLine($"Результат: {lockedCounter}");
	}
}