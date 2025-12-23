using System;
using System.Diagnostics;
using System.Threading;

class Program
{
	static void Main()
	{
		// Создаем и запускаем рабочий поток
		Thread workerThread = new Thread(DoWork);
		workerThread.Start();

		// Главный поток продолжает выполнение
		for (int i = 0; i < 5; i++)
		{
			Console.WriteLine("Работа основного потока");
			Thread.Sleep(500); // Приостановка на 500 мс
		}

		// Дополнительная логика для демонстрации работы потоков
		MonitorThreads(workerThread);

	}

	static void DoWork()
	{
		for (int i = 0; i < 5; i++)
		{
			Console.WriteLine("Работа второго потока");
			Thread.Sleep(500); // Приостановка на 500 мс
		}

		// Дополнительное завершение работы потока
		Console.WriteLine("Второй поток завершил свою работу");
	}

	static void MonitorThreads(Thread workerThread)
	{
		// Показываем информацию о выполнении потоков
		Console.WriteLine("\n=== Мониторинг потоков ===");
		Console.WriteLine($"Главный поток ID: {Thread.CurrentThread.ManagedThreadId}");
		Console.WriteLine($"Рабочий поток ID: {workerThread.ManagedThreadId}");
		Console.WriteLine($"Рабочий поток состояние: {workerThread.ThreadState}");

		// Ждем завершения рабочего потока если он еще не завершился
		if (workerThread.IsAlive)
		{
			Console.WriteLine("\nОжидание завершения рабочего потока...");

			// Ожидаем завершения с таймаутом
			bool completed = workerThread.Join(TimeSpan.FromSeconds(3));

			if (completed)
			{
				Console.WriteLine("Рабочий поток успешно завершился");
			}
			else
			{
				Console.WriteLine("Таймаут ожидания рабочего потока");
				Console.WriteLine($"Текущее состояние: {workerThread.ThreadState}");
			}
		}
		else
		{
			Console.WriteLine("\nРабочий поток уже завершился");
		}

		// Демонстрация состояния после выполнения
		Console.WriteLine("\n=== Финальное состояние ===");
		Console.WriteLine($"Главный поток: {Thread.CurrentThread.ThreadState}");
		Console.WriteLine($"Рабочий поток: {workerThread.ThreadState}");

		// Показываем статистику выполнения
		Console.WriteLine("\n=== Статистика выполнения ===");

		// Получаем информацию о процессе для демонстрации
		ProcessThreadCollection threads = System.Diagnostics.Process.GetCurrentProcess().Threads;
		Console.WriteLine($"Всего потоков в процессе: {threads.Count}");

		// Демонстрация приоритетов
		Console.WriteLine($"\nПриоритет главного потока: {Thread.CurrentThread.Priority}");
		Console.WriteLine($"Приоритет рабочего потока: {workerThread.Priority}");

		// Рекомендации по работе с потоками
		Console.WriteLine("\n=== Рекомендации ===");
		Console.WriteLine("1. Всегда давайте потокам осмысленные имена");
		Console.WriteLine("2. Используйте фоновые потоки для задач, которые могут быть прерваны");
		Console.WriteLine("3. Избегайте Thread.Sleep() в продакшен коде");
		Console.WriteLine("4. Рассмотрите использование Task для более эффективного управления");

		// Демонстрация именования потоков
		workerThread.Name = "Рабочий поток (DoWork)";
		Thread.CurrentThread.Name = "Главный поток";

		Console.WriteLine($"\nИменованные потоки:");
		Console.WriteLine($"- {Thread.CurrentThread.Name ?? "Без имени"}");
		Console.WriteLine($"- {workerThread.Name ?? "Без имени"}");
	}
}