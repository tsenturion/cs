using System;
using System.Threading;

class Program
{
	static void Main()
	{
		// Главный поток начинает выполнение
		Console.WriteLine("Начало работы программы");

		// Показываем информацию о потоке
		ShowThreadInfo("Главный поток");

		// Синхронное выполнение - поток блокируется
		Console.WriteLine("\nВызов Thread.Sleep(3000)...");
		Thread.Sleep(3000); // Поток блокируется на 3 секунды

		Console.WriteLine("Поток возобновил работу после Sleep");

		// Демонстрация стека вызовов
		DemonstrateCallStack();

		// Симуляция длительной операции
		SimulateLongOperation();

		Console.WriteLine("\nКонец работы программы");
	}

	static void ShowThreadInfo(string context)
	{
		Thread currentThread = Thread.CurrentThread;
		Console.WriteLine($"{context}:");
		Console.WriteLine($"  ID потока: {currentThread.ManagedThreadId}");
		Console.WriteLine($"  Приоритет: {currentThread.Priority}");
		Console.WriteLine($"  Состояние: {currentThread.ThreadState}");
	}

	static void DemonstrateCallStack()
	{
		Console.WriteLine("\n=== Демонстрация стека вызовов ===");

		Console.WriteLine("Метод Main вызывает MethodA");
		MethodA();

		Console.WriteLine("Возврат в Main");
	}

	static void MethodA()
	{
		Console.WriteLine("  MethodA: Зашел в метод");
		Console.WriteLine("  MethodA: Вызываю MethodB");

		MethodB();

		Console.WriteLine("  MethodA: Вернулся из MethodB");
		Console.WriteLine("  MethodA: Завершаю работу");
	}

	static void MethodB()
	{
		Console.WriteLine("    MethodB: Зашел в метод");
		Console.WriteLine("    MethodB: Выполняю работу...");
		Thread.Sleep(500);
		Console.WriteLine("    MethodB: Завершаю работу");
	}

	static void SimulateLongOperation()
	{
		Console.WriteLine("\n=== Симуляция длительной операции ===");

		Console.WriteLine("Начинаю длительную операцию (чтение файла)...");
		DateTime start = DateTime.Now;

		// Имитация чтения большого файла
		for (int i = 1; i <= 5; i++)
		{
			Console.WriteLine($"  Чтение файла... часть {i}/5");
			Thread.Sleep(1000); // Блокировка потока на 1 секунду
		}

		TimeSpan duration = DateTime.Now - start;
		Console.WriteLine($"Длительная операция завершена за {duration.TotalSeconds:F1} секунд");

		Console.WriteLine("\nПроблема: Поток был заблокирован все это время!");
		Console.WriteLine("Не мог обрабатывать другие запросы или обновлять UI.");
	}
}