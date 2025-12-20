using System;
using System.Diagnostics;

class Program
{
	static void Main()
	{
		// Получаем информацию о текущем процессе
		Process currentProcess = Process.GetCurrentProcess();

		// Выводим базовую информацию о процессе
		Console.WriteLine("Имя процесса: " + currentProcess.ProcessName);
		Console.WriteLine("Идентификатор процесса (PID): " + currentProcess.Id);
		Console.WriteLine("Время запуска процесса: " + currentProcess.StartTime);
		Console.WriteLine("Используемая память (байты): " + currentProcess.WorkingSet64);

		// Дополнительная информация для демонстрации (без изменения структуры)
		Console.WriteLine("\n=== Дополнительная информация ===");
		Console.WriteLine("Всего времени процессора: " + currentProcess.TotalProcessorTime);
		Console.WriteLine("Приоритет процесса: " + currentProcess.PriorityClass);
		Console.WriteLine("Количество потоков: " + currentProcess.Threads.Count);

		// Форматированный вывод памяти
		Console.WriteLine("\n=== Использование памяти ===");
		Console.WriteLine("Рабочий набор: " + FormatBytes(currentProcess.WorkingSet64));
		Console.WriteLine("Частные байты: " + FormatBytes(currentProcess.PrivateMemorySize64));
		Console.WriteLine("Виртуальная память: " + FormatBytes(currentProcess.VirtualMemorySize64));
	}

	// Вспомогательный метод для форматирования байтов в читаемый вид
	static string FormatBytes(long bytes)
	{
		string[] sizes = { "B", "KB", "MB", "GB", "TB" };
		double len = bytes;
		int order = 0;

		while (len >= 1024 && order < sizes.Length - 1)
		{
			order++;
			len = len / 1024;
		}

		return string.Format("{0:0.##} {1}", len, sizes[order]);
	}
}