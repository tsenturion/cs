using System;
using System.Threading;

class Program
{
	static void Main()
	{
		// Создаем новый поток, указывая метод для выполнения
		Thread thread = new Thread(DoWork);

		// Запускаем выполнение потока
		thread.Start();

		Console.WriteLine("Главный поток завершил работу");

		// Безопасное ожидание завершения потока (дополнение без изменения структуры)
		WaitForThreadCompletion(thread);
	}

	static void DoWork()
	{
		Console.WriteLine("Код выполняется в отдельном потоке");

		// Демонстрация дополнительной работы в потоке
		SimulateWork();
	}

	static void SimulateWork()
	{
		// Имитация работы в потоке
		Console.WriteLine("Поток начал выполнение работы...");

		for (int i = 1; i <= 5; i++)
		{
			Console.WriteLine($"  Поток: выполнение шага {i}/5");
			Thread.Sleep(500); // Имитация задержки
		}

		Console.WriteLine("Поток завершил выполнение работы");
	}

	static void WaitForThreadCompletion(Thread thread)
	{
		// Показываем информацию о состоянии потока
		Console.WriteLine($"\n=== Информация о созданном потоке ===");
		Console.WriteLine($"ID потока: {thread.ManagedThreadId}");
		Console.WriteLine($"Имя потока: {(thread.Name ?? "[не задано]")}");
		Console.WriteLine($"Приоритет: {thread.Priority}");
		Console.WriteLine($"Состояние: {thread.ThreadState}");
		Console.WriteLine($"Фоновый поток: {thread.IsBackground}");

		// Даем выбор: ждать завершения или продолжить
		Console.WriteLine("\nДождаться завершения потока? (y/n)");
		var key = Console.ReadKey(true).KeyChar;

		if (char.ToLower(key) == 'y')
		{
			Console.WriteLine("\nОжидание завершения потока...");

			// Ожидаем завершения потока с таймаутом
			bool completed = thread.Join(TimeSpan.FromSeconds(10));

			if (completed)
			{
				Console.WriteLine("Поток успешно завершился");
			}
			else
			{
				Console.WriteLine("Таймаут ожидания потока. Поток все еще выполняется.");
				Console.WriteLine($"Текущее состояние: {thread.ThreadState}");
			}
		}
		else
		{
			Console.WriteLine("\nПродолжаем выполнение без ожидания потока");

			// Если поток фоновый, он завершится при закрытии приложения
			// Если поток не фоновый, приложение будет работать пока поток не завершится
			if (!thread.IsBackground)
			{
				Console.WriteLine("ВНИМАНИЕ: Созданный поток НЕ фоновый.");
				Console.WriteLine("Приложение будет работать пока поток не завершится.");
			}
		}

		// Показываем итоговое состояние
		Console.WriteLine($"\nФинальное состояние потока: {thread.ThreadState}");

		// Демонстрация пула потоков для сравнения
		Console.WriteLine("\n=== Сравнение с пулом потоков ===");
		Console.WriteLine("Пул потоков обычно используется для коротких задач:");

		ThreadPool.QueueUserWorkItem(state =>
		{
			Console.WriteLine("Задача выполняется в пуле потоков");
			Console.WriteLine($"ID потока пула: {Thread.CurrentThread.ManagedThreadId}");
			Thread.Sleep(1000);
			Console.WriteLine("Задача пула потоков завершена");
		});

		// Короткая задержка для демонстрации работы пула
		Thread.Sleep(200);
	}
}