using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        List<object> collection = new List<object>
        {
            42,
            "Hello, World!",
            3.14,
            DateTime.Now
        };

        Thread thread = new Thread(() => ProcessCollection(collection));

        thread.Start();

        thread.Join();

        Console.WriteLine("Поток завершил работу.");
    }

    static void ProcessCollection(IEnumerable<object> collection)
    {
        foreach (var item in collection)
        {
            Console.WriteLine(item.ToString());
        }
    }
}