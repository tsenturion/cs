using System;
using System.Diagnostics;
using System.Threading;

class Program
{
	static void Main()
	{
		// Получаем PID текущего процесса
		Console.WriteLine("PID процесса: " + Process.GetCurrentProcess().Id);

		// Получаем ID текущего управляемого потока
		Console.WriteLine("ID текущего потока: " + Thread.CurrentThread.ManagedThreadId);

		// Дополнительная информация о процессах и потоках (без изменения структуры)
		Console.WriteLine("\n=== Дополнительная информация ===");

		// Информация о процессе
		Process currentProcess = Process.GetCurrentProcess();
		Console.WriteLine("Имя процесса: " + currentProcess.ProcessName);
		Console.WriteLine("Время запуска: " + currentProcess.StartTime);
		Console.WriteLine("Приоритет процесса: " + currentProcess.BasePriority);
		Console.WriteLine("Количество потоков в процессе: " + currentProcess.Threads.Count);

		// Информация о текущем потоке
		Thread currentThread = Thread.CurrentThread;
		Console.WriteLine("\nИнформация о текущем потоке:");
		Console.WriteLine("Имя потока: " + (currentThread.Name ?? "[не задано]"));
		Console.WriteLine("Состояние потока: " + currentThread.ThreadState);
		Console.WriteLine("Приоритет потока: " + currentThread.Priority);
		Console.WriteLine("Фоновый ли поток: " + currentThread.IsBackground);
		Console.WriteLine("Пул потоков: " + currentThread.IsThreadPoolThread);

		// Информация о системе
		Console.WriteLine("\n=== Системная информация ===");
		Console.WriteLine("Количество процессоров: " + Environment.ProcessorCount);
		Console.WriteLine("Версия CLR: " + Environment.Version);
		Console.WriteLine("64-битный процесс: " + Environment.Is64BitProcess);

		// Создаем еще один поток для демонстрации
		Console.WriteLine("\n=== Создание дополнительного потока ===");
		Thread secondThread = new Thread(() =>
		{
			Console.WriteLine("Дополнительный поток:");
			Console.WriteLine("  ID потока: " + Thread.CurrentThread.ManagedThreadId);
			Console.WriteLine("  Приоритет: " + Thread.CurrentThread.Priority);
			Console.WriteLine("  Фоновый: " + Thread.CurrentThread.IsBackground);

			// Имитация работы
			Thread.Sleep(500);
			Console.WriteLine("  Дополнительный поток завершил работу");
		});

		secondThread.Name = "Демонстрационный поток";
		secondThread.IsBackground = true; // Фоновый поток

		Console.WriteLine("Запуск дополнительного потока...");
		secondThread.Start();

		// Ждем завершения дополнительного потока
		secondThread.Join();

		// Показываем статистику потоков пула
		Console.WriteLine("\n=== Статистика пула потоков ===");
		ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
		ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);

		Console.WriteLine("Пул потоков (рабочие):");
		Console.WriteLine($"  Максимум: {maxWorkerThreads}");
		Console.WriteLine($"  Доступно: {availableWorkerThreads}");
		Console.WriteLine($"  Используется: {maxWorkerThreads - availableWorkerThreads}");

		Console.WriteLine("\nПул потоков (порты завершения ввода-вывода):");
		Console.WriteLine($"  Максимум: {maxCompletionPortThreads}");
		Console.WriteLine($"  Доступно: {availableCompletionPortThreads}");
		Console.WriteLine($"  Используется: {maxCompletionPortThreads - availableCompletionPortThreads}");
	}
}