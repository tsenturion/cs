using System.Diagnostics;

namespace Program
{
	class Program
	{
		static void Main()
		{
			Process process = new();
			process.StartInfo = new ProcessStartInfo("notepad.exe");
			Console.WriteLine("Процесс запущен");
			process.Start();
			process.WaitForExit();
			Console.WriteLine($"Процесс остановлен, код процесса: {process.GetHashCode()}");
		}
	}
}