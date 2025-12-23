using System;
using System.Threading;

class Program
{
	// ManualResetEvent - примитив синхронизации для управления потоками
	// true - изначально в сигнальном состоянии (поток может работать)
	static ManualResetEvent pauseEvent = new ManualResetEvent(true);

	static void Main()
	{
		// Создаем и запускаем рабочий поток
		Thread thread = new Thread(Work);
		thread.Start();

		// Даем потоку поработать 2 секунды
		Thread.Sleep(2000);
		Console.WriteLine("Приостанавливаем поток");
		pauseEvent.Reset(); // Переводим в несигнальное состояние

		// Держим паузу 2 секунды
		Thread.Sleep(2000);
		Console.WriteLine("Возобновляем поток");
		pauseEvent.Set(); // Переводим в сигнальное состояние

		// Ждем завершения потока
		thread.Join();
		Console.WriteLine("Работа завершена");

		// Демонстрация дополнительных возможностей
		DemonstrateResetEventFeatures();
	}

	static void Work()
	{
		for (int i = 1; i <= 5; i++)
		{
			// Ждем, пока событие не перейдет в сигнальное состояние
			pauseEvent.WaitOne();
			Console.WriteLine($"Итерация {i}");
			Thread.Sleep(1000);
		}

		Console.WriteLine("Рабочий поток завершен");
	}

	static void DemonstrateResetEventFeatures()
	{
		Console.WriteLine("\n--- Демонстрация ResetEvent ---");

		// Создаем несколько событий для разных сценариев
		ManualResetEvent event1 = new ManualResetEvent(false);
		ManualResetEvent event2 = new ManualResetEvent(false);

		// Демонстрация WaitOne с таймаутом
		Thread testThread = new Thread(() =>
		{
			Console.WriteLine("Поток: Ожидаю событие 1...");
			bool signaled = event1.WaitOne(TimeSpan.FromSeconds(2));

			if (signaled)
			{
				Console.WriteLine("Поток: Событие 1 получено");
			}
			else
			{
				Console.WriteLine("Поток: Таймаут ожидания события 1");
			}

			Console.WriteLine("Поток: Ожидаю событие 2...");
			event2.WaitOne();
			Console.WriteLine("Поток: Событие 2 получено");
		});

		testThread.Start();

		// Запускаем события в разное время
		Thread.Sleep(1500);
		event1.Set(); // Устанавливаем первое событие

		Thread.Sleep(1000);
		event2.Set(); // Устанавливаем второе событие

		testThread.Join();

		// Демонстрация AutoResetEvent (одноразовое событие)
		Console.WriteLine("\n--- AutoResetEvent ---");
		AutoResetEvent autoEvent = new AutoResetEvent(false);

		Thread autoThread = new Thread(() =>
		{
			for (int i = 0; i < 3; i++)
			{
				autoEvent.WaitOne();
				Console.WriteLine($"AutoResetEvent: получено {i + 1}");
			}
		});

		autoThread.Start();

		// AutoResetEvent автоматически сбрасывается после каждого WaitOne
		for (int i = 0; i < 3; i++)
		{
			Thread.Sleep(500);
			autoEvent.Set();
		}

		autoThread.Join();

		// Очистка ресурсов
		event1.Dispose();
		event2.Dispose();
		autoEvent.Dispose();
	}
}