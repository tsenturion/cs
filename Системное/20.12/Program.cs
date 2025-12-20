using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lesson_2__first_with_practice_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Текущий процесс
            Process CurrentProcess = Process.GetCurrentProcess();

            Console.WriteLine("--- Информация о текущем процессе ---");
            Console.WriteLine($"PID процесса: {CurrentProcess.Id}");
            Console.WriteLine($"Имя процесса: {CurrentProcess.ProcessName}");
            Console.WriteLine($"Время запуска: {CurrentProcess.StartTime}");
            Console.WriteLine($"Количество потоков: {CurrentProcess.Threads.Count}");

            Console.WriteLine("\n--- Запуск внешнего процесса (блокнот) ---");

            // Создание набора значений для процесса перед его запуском
            ProcessStartInfo NewProcessInfo = new ProcessStartInfo
            {
                // Путь к файлу для запуска
                FileName = "notepad.exe",
                // Отключение использования оболочки ОС для процесса
                UseShellExecute = false,
                // Перенаправление стандартных поток, чтобы дочерний и родительский процессы могли обмениваться между собой данными
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                // Отключение запуска процесса в новом окне
                CreateNoWindow = true
            };

            // Запуск дочернего процесса с необходимым параметрами
            using (Process NewProcess = new Process { StartInfo = NewProcessInfo })
            {
                NewProcess.Start();
                Console.WriteLine($"PID дочернего процесса: {NewProcess.Id}");
                if (Process.GetProcessById(NewProcess.Id) != null) Console.WriteLine("Процесс запущен");
                else Console.WriteLine("Процесс НЕ запущен");

                Console.WriteLine("Ожидание завершения процесса");
                NewProcess.WaitForExit();

                Console.WriteLine($"\nПроцесс завершился с кодом {NewProcess.ExitCode}");
            }
        }
    }
}