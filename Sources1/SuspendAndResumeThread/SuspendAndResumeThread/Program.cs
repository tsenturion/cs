using System;
using System.Threading;

namespace SuspendAndResumeThread
{
    class Program
    {
		static ManualResetEvent _pauseEvent = new ManualResetEvent(true); // Поток по умолчанию работает!!!!!

		static void Main(string[] args)
		{
			/*
			* реализовать возможность множественной приостановки и продолжения!!!
			* */ 
			ThreadStart ts = new ThreadStart(Method);
			Thread t = new Thread(ts);
			t.Start(); 

			Console.WriteLine("Нажмите 'p' для приостановки, 'r' для возобновления, 'e' для выхода.");
			ConsoleKeyInfo key;
			do
			{
				key = Console.ReadKey(true);
				switch (key.KeyChar)
				{
					case 'p':
					case 'P':
						Console.WriteLine("Поток приостановлен.");
						_pauseEvent.Reset(); // Ставим поток в режим ожидания
						break;
					case 'r':
					case 'R':
						Console.WriteLine("Поток возобновлен.");
						_pauseEvent.Set(); // Поток может работать дальше
						break;
					case 'e':
					case 'E':
						Console.WriteLine("Завершение программы...");
						_pauseEvent.Set(); // Позволяем потоку завершиться
						break;
				}
			} while (key.KeyChar != 'e' && key.KeyChar != 'E');

			t.Join(); // Дожидаемся завершения потока
			Console.WriteLine("Программа завершена.");
		}

		static void Method()
		{
			for (int i = 0; i < 1000; i++)
			{
				_pauseEvent.WaitOne(); // Останавливаемся, если сигнал не установлен
				Console.WriteLine(i);
				Thread.Sleep(100);
			}
		}
	}
}
