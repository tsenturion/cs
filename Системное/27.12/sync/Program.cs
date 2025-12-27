using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Класс Monitor ===\n");

		// 1. Монитор как основа lock
		Console.WriteLine("1. Monitor - основа lock:");
		await MonitorAsLockFoundation();

		// 2. Wait и Pulse - координация потоков
		Console.WriteLine("\n2. Wait и Pulse - координация потоков:");
		await WaitPulseExample();

		// 3. Producer-Consumer паттерн
		Console.WriteLine("\n3. Producer-Consumer паттерн:");
		await ProducerConsumerExample();

		// 4. TryEnter с таймаутом
		Console.WriteLine("\n4. TryEnter с таймаутом:");
		await TryEnterWithTimeout();

		// 5. Сравнение с lock
		Console.WriteLine("\n5. Сравнение с lock:");
		await CompareWithLock();

		// 6. Pulse vs PulseAll
		Console.WriteLine("\n6. Pulse vs PulseAll:");
		await PulseVsPulseAll();

		Console.WriteLine("\n=== Ключевые выводы ===");
		Console.WriteLine("- Monitor - низкоуровневый механизм синхронизации");
		Console.WriteLine("- lock - обертка над Monitor");
		Console.WriteLine("- Wait/Pulse для координации потоков");
		Console.WriteLine("- Всегда используйте try/finally с Monitor");
		Console.WriteLine("- Ожидание должно быть в цикле while");
	}

	// 1. Монитор как основа lock
	static async Task MonitorAsLockFoundation()
	{
		object locker = new object();
		int counter = 0;

		Console.WriteLine("  Пример 1: lock (скрытый Monitor)");
		Task lockTask = Task.Run(() =>
		{
			for (int i = 0; i < 1000; i++)
			{
				lock (locker)
				{
					counter++;
				}
			}
		});

		Console.WriteLine("  Пример 2: Monitor напрямую");
		Task monitorTask = Task.Run(() =>
		{
			for (int i = 0; i < 1000; i++)
			{
				Monitor.Enter(locker);
				try
				{
					counter++;
				}
				finally
				{
					Monitor.Exit(locker);
				}
			}
		});

		await Task.WhenAll(lockTask, monitorTask);
		Console.WriteLine($"  Итоговое значение счетчика: {counter}");
		Console.WriteLine("  Оба подхода дают одинаковый результат");
	}

	// 2. Wait и Pulse - координация потоков
	static async Task WaitPulseExample()
	{
		object locker = new object();
		bool condition = false;

		Console.WriteLine("  Создаем два потока...");

		// Поток, который ждет условия
		Task waiterTask = Task.Run(() =>
		{
			Console.WriteLine("    Waiter: Начинаю ожидание");
			lock (locker)
			{
				while (!condition)
				{
					Console.WriteLine("    Waiter: Вызываю Monitor.Wait()");
					Monitor.Wait(locker); // Освобождает монитор и ждет
					Console.WriteLine("    Waiter: Проснулся после Wait");
				}
				Console.WriteLine("    Waiter: Условие выполнено!");
			}
		});

		// Даем времени начать ожидание
		await Task.Delay(500);

		// Поток, который устанавливает условие
		Task signalerTask = Task.Run(() =>
		{
			Console.WriteLine("    Signaler: Устанавливаю условие");
			lock (locker)
			{
				condition = true;
				Console.WriteLine("    Signaler: Вызываю Monitor.Pulse()");
				Monitor.Pulse(locker); // Сигнализируем одному потоку
				Console.WriteLine("    Signaler: Монитор освобожден");
			}
		});

		await Task.WhenAll(waiterTask, signalerTask);
		Console.WriteLine("  Координация завершена");
	}

	// 3. Producer-Consumer паттерн
	static async Task ProducerConsumerExample()
	{
		object locker = new object();
		Queue<int> queue = new Queue<int>();
		bool productionComplete = false;

		Console.WriteLine("  Создаем Producer и Consumer...");

		// Producer
		Task producerTask = Task.Run(() =>
		{
			for (int i = 1; i <= 5; i++)
			{
				lock (locker)
				{
					Console.WriteLine($"    Producer: Добавляю {i} в очередь");
					queue.Enqueue(i);
					Monitor.Pulse(locker); // Сигнал потребителю
					Console.WriteLine($"    Producer: Отправил сигнал Pulse");
				}
				Thread.Sleep(300);
			}

			lock (locker)
			{
				productionComplete = true;
				Monitor.PulseAll(locker); // Сигнал всем потребителям
				Console.WriteLine("    Producer: Завершил производство");
			}
		});

		// Consumer
		Task consumerTask = Task.Run(() =>
		{
			int consumedCount = 0;
			while (true)
			{
				lock (locker)
				{
					// Цикл while, а не if!
					while (queue.Count == 0 && !productionComplete)
					{
						Console.WriteLine("    Consumer: Очередь пуста, жду...");
						Monitor.Wait(locker);
					}

					if (queue.Count > 0)
					{
						int item = queue.Dequeue();
						consumedCount++;
						Console.WriteLine($"    Consumer: Получил {item} (всего: {consumedCount})");
					}

					if (productionComplete && queue.Count == 0)
					{
						Console.WriteLine($"    Consumer: Завершил работу");
						break;
					}
				}
			}
		});

		await Task.WhenAll(producerTask, consumerTask);
		Console.WriteLine("  Producer-Consumer завершен");
	}

	// 4. TryEnter с таймаутом
	static async Task TryEnterWithTimeout()
	{
		object locker = new object();
		bool lockTaken = false;

		Console.WriteLine("  Сценарий 1: Долгая блокировка");

		// Поток, который долго держит блокировку
		Task longHolder = Task.Run(() =>
		{
			lock (locker)
			{
				Console.WriteLine("    LongHolder: Захватил блокировку на 2 секунды");
				Thread.Sleep(2000);
				Console.WriteLine("    LongHolder: Освобождаю блокировку");
			}
		});

		// Даем времени захватить блокировку
		await Task.Delay(100);

		Task tryEnterTask = Task.Run(() =>
		{
			Console.WriteLine("    TryEnter: Пытаюсь захватить блокировку...");

			// Попытка захвата с таймаутом 500 мс
			lockTaken = Monitor.TryEnter(locker, 500);
			try
			{
				if (lockTaken)
				{
					Console.WriteLine("    TryEnter: Блокировка получена!");
				}
				else
				{
					Console.WriteLine("    TryEnter: Не удалось получить блокировку за 500 мс");
				}
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(locker);
					Console.WriteLine("    TryEnter: Освободил блокировку");
				}
			}
		});

		await Task.WhenAll(longHolder, tryEnterTask);

		Console.WriteLine("\n  Сценарий 2: Немедленный захват");
		lockTaken = false;

		// Теперь блокировка свободна
		lockTaken = Monitor.TryEnter(locker);
		if (lockTaken)
		{
			Console.WriteLine("    TryEnter: Блокировка получена немедленно");
			Monitor.Exit(locker);
		}
	}

	// 5. Сравнение с lock
	static async Task CompareWithLock()
	{
		Console.WriteLine("  Сравнение Monitor и lock:");

		object locker = new object();

		// lock - удобный синтаксический сахар
		Console.WriteLine("\n  lock (синтаксический сахар):");
		Console.WriteLine("    lock (locker)");
		Console.WriteLine("    {");
		Console.WriteLine("        // критическая секция");
		Console.WriteLine("    }");

		// Компилятор разворачивает это в:
		Console.WriteLine("\n  Monitor (что на самом деле):");
		Console.WriteLine("    Monitor.Enter(locker);");
		Console.WriteLine("    try");
		Console.WriteLine("    {");
		Console.WriteLine("        // критическая секция");
		Console.WriteLine("    }");
		Console.WriteLine("    finally");
		Console.WriteLine("    {");
		Console.WriteLine("        Monitor.Exit(locker);");
		Console.WriteLine("    }");

		Console.WriteLine("\n  Преимущества lock:");
		Console.WriteLine("    - Проще и читабельнее");
		Console.WriteLine("    - Гарантированное освобождение");
		Console.WriteLine("    - Меньше шансов на ошибку");

		Console.WriteLine("\n  Когда нужен Monitor:");
		Console.WriteLine("    - Wait/Pulse для координации");
		Console.WriteLine("    - TryEnter с таймаутом");
		Console.WriteLine("    - Сложные сценарии синхронизации");
	}

	// 6. Pulse vs PulseAll
	static async Task PulseVsPulseAll()
	{
		object locker = new object();
		bool ready = false;
		int waitersCount = 3;

		Console.WriteLine($"  Создаем {waitersCount} ожидающих потока...");

		List<Task> waiterTasks = new List<Task>();
		for (int i = 1; i <= waitersCount; i++)
		{
			int id = i;
			waiterTasks.Add(Task.Run(() =>
			{
				lock (locker)
				{
					Console.WriteLine($"    Waiter {id}: Начинаю ожидание");
					while (!ready)
					{
						Monitor.Wait(locker);
					}
					Console.WriteLine($"    Waiter {id}: Проснулся!");
				}
			}));
		}

		// Даем всем потокам начать ожидание
		await Task.Delay(500);

		Console.WriteLine("\n  Тест 1: Pulse (будит один поток)");
		lock (locker)
		{
			ready = true;
			Console.WriteLine("    Signal: Вызываю Monitor.Pulse()");
			Monitor.Pulse(locker);
		}

		await Task.Delay(1000);
		Console.WriteLine("    Результат: Проснулся только один поток");

		Console.WriteLine("\n  Тест 2: PulseAll (будит все потоки)");
		ready = false; // Сбрасываем для второго теста

		// Снова запускаем ожидание
		waiterTasks.Clear();
		for (int i = 1; i <= waitersCount; i++)
		{
			int id = i;
			waiterTasks.Add(Task.Run(() =>
			{
				lock (locker)
				{
					while (!ready)
					{
						Monitor.Wait(locker);
					}
					Console.WriteLine($"    Waiter {id}: Проснулся от PulseAll!");
				}
			}));
		}

		await Task.Delay(500);

		lock (locker)
		{
			ready = true;
			Console.WriteLine("    Signal: Вызываю Monitor.PulseAll()");
			Monitor.PulseAll(locker);
		}

		await Task.WhenAll(waiterTasks);
		Console.WriteLine("    Результат: Проснулись все потоки");
	}
}