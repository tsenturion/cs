using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel_and_PLINQ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<int> Numbers = new List<int>();
            Random random = new Random();
            int TotalResult = 0;
            Stopwatch stopwatch = new Stopwatch();

            Console.Write("Массив чисел: ");

            for (int i = 0; i < 100; i++)
            {
                int numberToAdd = random.Next(1, 10000000);
                Numbers.Add(numberToAdd);
                Console.Write(numberToAdd + "; ");
            }
            Console.WriteLine();

            Console.WriteLine("\nПример обычного вычисления:");
            stopwatch.Start();
            foreach (int Number in Numbers)
            {
                TotalResult += Number;
            }
            stopwatch.Stop();
            Console.WriteLine($"Результат = {TotalResult}");
            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed}"); //в легких задачах будет быстрее параллельных вариантов
            TotalResult = 0;

            Console.WriteLine("\nПример неправильного параллельного вычисления:");
            stopwatch.Start();
            Parallel.ForEach(Numbers, number =>
            {
                TotalResult += number;
            });
            stopwatch.Stop();
            Console.WriteLine($"Результат = {TotalResult}");
            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed}");
            TotalResult = 0;

            Console.WriteLine("\nПример правильного параллельного вычисления (с локальными аккумуляторами):");
            stopwatch.Start();
            Parallel.ForEach(Numbers,
                () => 0,
                (number, loopState, localResult) =>
                                            {
                                                localResult += number;
                                                return localResult;
                                            },
                (localResult) =>
            {
                Interlocked.Add(ref TotalResult, localResult);  
            });
            stopwatch.Stop();
            Console.WriteLine($"Результат = {TotalResult}");
            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed}");
            TotalResult = 0;

            Console.WriteLine("\nПример параллельного вычисления с помощью PLINQ:");
            stopwatch.Start();
            TotalResult = Numbers.AsParallel().Sum();
            stopwatch.Stop();
            Console.WriteLine($"Результат = {TotalResult}");
            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed}");
            TotalResult = 0;

            Console.WriteLine("\nПример параллельного вычисления с помощью PLINQ и с AsOrdered():");
            stopwatch.Start();
            //AsOrdered() немного замедляет выполнение функции, но обрабатывает источник данных, как упорядоченный
            TotalResult = Numbers.AsParallel().AsOrdered().Sum();
            stopwatch.Stop();
            Console.WriteLine($"Результат = {TotalResult}");
            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed}");
            TotalResult = 0;
        }
    }
}
