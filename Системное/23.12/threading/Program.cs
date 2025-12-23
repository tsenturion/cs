using System;
using System.Threading;

class Program
{
	// Флаг для корректного завершения потока
	static bool shouldStop = false;

	static void Main()
	{
		// Создаем и запускаем рабочий поток
		Thread thread = new Thread(Work);
		thread.Start();

		// Даем потоку поработать 3 секунды
		Thread.Sleep(3000);
		Console.WriteLine("Запрашиваем завершение потока");
		shouldStop = true; // Устанавливаем флаг завершения

		// Ждем завершения потока
		thread.Join();
		Console.WriteLine("Поток корректно завершён");

		// Демонстрация с использованием CancellationToken
		DemonstrateCancellationToken();
	}

	static void Work()
	{
		while (!shouldStop) // Проверяем флаг в каждой итерации
		{
			Console.WriteLine("Поток работает");
			Thread.Sleep(1000);
		}

		Console.WriteLine("Поток получил сигнал завершения");
	}

	static void DemonstrateCancellationToken()
	{
		Console.WriteLine("\n--- CancellationToken (рекомендуемый способ) ---");

		// Создаем источник токена отмены
		CancellationTokenSource cts = new CancellationTokenSource();
		CancellationToken token = cts.Token;

		Thread tokenThread = new Thread(() =>
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					Console.WriteLine("Поток с токеном работает");
					Thread.Sleep(500);
				}
				Console.WriteLine("Поток: Токен отмены получен");
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Поток: Прерван через исключение");
			}
		});

		tokenThread.Start();

		// Даем поработать 2 секунды
		Thread.Sleep(2000);
		Console.WriteLine("Запрашиваем отмену через CancellationTokenSource");
		cts.Cancel();

		tokenThread.Join();

		// Демонстрация отмены с таймаутом
		Console.WriteLine("\n--- Отмена с таймаутом ---");
		CancellationTokenSource cts2 = new CancellationTokenSource();

		Thread timeoutThread = new Thread(() =>
		{
			int count = 0;
			while (!cts2.Token.IsCancellationRequested)
			{
				Console.WriteLine($"Таймаут поток: {++count}");
				Thread.Sleep(1000);
			}
			Console.WriteLine("Таймаут поток завершен");
		});

		timeoutThread.Start();
		cts2.CancelAfter(3500); // Автоматическая отмена через 3.5 секунды

		timeoutThread.Join();

		// Исправленная демонстрация с volatile полем класса
		Console.WriteLine("\n--- Volatile поле класса ---");
		var flagDemo = new FlagDemo();
		flagDemo.RunDemo();

		// Очистка ресурсов
		cts.Dispose();
		cts2.Dispose();
	}
}

// Класс для демонстрации volatile поля
class FlagDemo
{
	// Volatile можно использовать только для полей класса
	private volatile bool stopFlag = false;

	public void RunDemo()
	{
		Thread safeThread = new Thread(() =>
		{
			while (!stopFlag)
			{
				Console.WriteLine("Volatile поток работает");
				Thread.Sleep(300);
			}
			Console.WriteLine("Volatile поток завершен");
		});

		safeThread.Start();
		Thread.Sleep(1500);

		// Изменение volatile поля видно сразу всем потокам
		stopFlag = true;
		safeThread.Join();
	}
}