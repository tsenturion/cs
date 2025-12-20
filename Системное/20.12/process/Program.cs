using System;
using System.Diagnostics;

class Program
{
	static void Main()
	{
		// Создаем информацию для запуска нового процесса
		ProcessStartInfo startInfo = new ProcessStartInfo();
		startInfo.FileName = "notepad.exe"; // Указываем исполняемый файл для запуска

		// Запускаем процесс с указанными параметрами
		Process process = Process.Start(startInfo);

		// Выводим идентификатор запущенного процесса
		Console.WriteLine("Запущен процесс с PID: " + process.Id);

		// Добавляем информацию о состоянии процесса (без изменения структуры)
		Console.WriteLine("\n=== Информация о запущенном процессе ===");
		Console.WriteLine("Имя процесса: " + process.ProcessName);
		Console.WriteLine("Время запуска: " + process.StartTime);
		Console.WriteLine("Приоритет: " + process.PriorityClass);

		// Ожидаем завершения или предоставляем управление
		Console.WriteLine("\nВыберите действие:");
		Console.WriteLine("1 - Ожидать завершения процесса");
		Console.WriteLine("2 - Продолжить без ожидания");
		Console.WriteLine("3 - Завершить процесс");

		var key = Console.ReadKey(true).KeyChar;
		Console.WriteLine();

		switch (key)
		{
			case '1':
				// Ожидаем завершения процесса
				process.WaitForExit();
				Console.WriteLine($"Процесс завершен с кодом: {process.ExitCode}");
				Console.WriteLine($"Время работы: {process.TotalProcessorTime}");
				break;

			case '2':
				// Продолжаем выполнение, процесс работает независимо
				Console.WriteLine($"Процесс {process.ProcessName} (PID: {process.Id}) работает в фоне");
				break;

			case '3':
				// Завершаем процесс
				if (!process.HasExited)
				{
					process.Kill();
					Console.WriteLine("Процесс принудительно завершен");
				}
				break;

			default:
				Console.WriteLine("Неизвестная команда, продолжаем...");
				break;
		}

		// Всегда освобождаем ресурсы
		if (!process.HasExited)
		{
			Console.WriteLine("\nПроцесс все еще выполняется. Закройте Блокнот вручную.");
		}

		process.Dispose(); // Явное освобождение ресурсов
	}
}