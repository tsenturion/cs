using System;
using System.Threading;

namespace SuspendAndResumeThread
{
	class Program
	{
		static void Main(string[] args)
		{
			ThreadStart ts = new ThreadStart(Method);
			Thread t = new Thread(ts);
			t.Start(); // Запуск потока.

			ConsoleKeyInfo key;
			Console.WriteLine("Нажмите клавишу s для остановки");
			while (true)
			{
				key = Console.ReadKey();
				if (key.Key == ConsoleKey.R)
				{
					Console.WriteLine("\nНажмите клавишу s для остановки");
					t.Resume();
				}
				else if (key.Key == ConsoleKey.S)
				{
					Console.WriteLine("\nНажмите клавишу r для возобновления");
					t.Suspend();
				}
			}
		}

		static void Method()
		{
			for (int i = 0; i < 100; i++)
			{
				Console.WriteLine(i);
				Thread.Sleep(100);
			}
		}
	}
}
