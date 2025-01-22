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

            ConsoleKey key = new ConsoleKey();
            bool flag= true;

            while (key != ConsoleKey.Escape)
            {
                key = Console.ReadKey(true).Key;
                if (flag)
                {
                    t.Suspend(); // Приостановка потока.
                    Console.WriteLine("Поток остановлен!");
                    Console.WriteLine("Нажмите любую клавишу для возобновления");
                    flag = false;
                }
                else
                {
                  t.Resume(); // Возобновление работы.
                  Console.WriteLine("Нажмите любую клавишу для остановки");
                    flag = true;
                }
            }
            t.Interrupt();
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
