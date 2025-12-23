using System;
using System.Threading;

class Program
{
	static void Main()
	{
		// Создание объекта потока
		Thread thread = new Thread(Work);
		Console.WriteLine("Поток создан, но ещё не запущен");

		// Запуск потока
		thread.Start();
		Console.WriteLine("Поток запущен");

		// Ожидание завершения потока
		thread.Join();
		Console.WriteLine("Основной поток дождался завершения");

		// Показываем финальное состояние
		Console.WriteLine($"Состояние потока: {thread.ThreadState}");
		Console.WriteLine($"ID потока: {thread.ManagedThreadId}");

		// Демонстрация без Join
		Console.WriteLine("\n--- Без Join ---");
		Thread thread2 = new Thread(QuickWork);
		thread2.Start();
		Console.WriteLine("Запущен второй поток без ожидания");
		Console.WriteLine("Основной поток завершается сразу");

		Thread.Sleep(100); // Даем время для вывода
	}

	static void Work()
	{
		Console.WriteLine("Код выполняется в новом потоке");
		Thread.Sleep(500);
		Console.WriteLine("Поток завершил работу");
	}

	static void QuickWork()
	{
		Console.WriteLine("Второй поток: старт");
		Thread.Sleep(200);
		Console.WriteLine("Второй поток: конец");
	}
}