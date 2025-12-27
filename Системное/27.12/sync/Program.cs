using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Mutex - межпроцессная синхронизация ===\n");

		// 1. Защита от запуска второго экземпляра
		Console.WriteLine("1. Один экземпляр приложения:");
		await SingleInstanceExample();

		// 2. Межпроцессная синхронизация
		Console.WriteLine("\n2. Межпроцессная синхронизация:");
		await CrossProcessExample();

		// 3. Сравнение производительности
		Console.WriteLine("\n3. Производительность:");
		await PerformanceComparison();

		// 4. AbandonedMutexException
		Console.WriteLine("\n4. AbandonedMutexException:");
		await AbandonedMutexDemo();

		// 5. Работа с файлом
		Console.WriteLine("\n5. Синхронизация доступа к файлу:");
		await FileAccessExample();
	}

	// 1. Защита от запуска второго экземпляра
	static async Task SingleInstanceExample()
	{
		bool createdNew;
		using (Mutex mutex = new Mutex(true, "Global\\MySingleInstanceApp", out createdNew))
		{
			if (!createdNew)
			{
				Console.WriteLine("Приложение уже запущено");
				Console.WriteLine("Закройте предыдущий экземпляр");
				return;
			}

			Console.WriteLine("Приложение запущено");
			Console.WriteLine("Попробуйте запустить второй экземпляр");

			await Task.Delay(3000);
			Console.WriteLine("Первый экземпляр завершает работу");
		}
	}

	// 2. Межпроцессная синхронизация
	static async Task CrossProcessExample()
	{
		Console.WriteLine("Процесс 1: Запущен");

		// Используем Task.Run для имитации разных потоков
		Task process1 = Task.Run(async () =>
		{
			using (Mutex mutex = new Mutex(false, "Global\\CrossProcessMutex"))
			{
				Console.WriteLine("Процесс 1: Захватываю мьютекс");
				mutex.WaitOne();

				try
				{
					Console.WriteLine("Процесс 1: Работаю с ресурсом");
					await Task.Delay(1500); // Работа с общим ресурсом
				}
				finally
				{
					Console.WriteLine("Процесс 1: Освобождаю мьютекс");
					mutex.ReleaseMutex();
				}
			}
		});

		// Запускаем "процесс 2" с задержкой
		await Task.Delay(500);

		Task process2 = Task.Run(async () =>
		{
			Console.WriteLine("Процесс 2: Запущен");

			try
			{
				using (Mutex mutex = Mutex.OpenExisting("Global\\CrossProcessMutex"))
				{
					Console.WriteLine("Процесс 2: Пытаюсь захватить мьютекс");
					bool acquired = mutex.WaitOne(1000);

					if (acquired)
					{
						try
						{
							Console.WriteLine("Процесс 2: Захватил мьютекс");
							await Task.Delay(500);
						}
						finally
						{
							mutex.ReleaseMutex();
							Console.WriteLine("Процесс 2: Освободил мьютекс");
						}
					}
					else
					{
						Console.WriteLine("Процесс 2: Не удалось захватить (таймаут)");
					}
				}
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				Console.WriteLine("Процесс 2: Мьютекс не найден");
			}
		});

		await Task.WhenAll(process1, process2);
		Console.WriteLine("Оба процесса завершены");
	}

	// 3. Сравнение производительности
	static async Task PerformanceComparison()
	{
		const int iterations = 10000;

		Console.WriteLine($"Тест на {iterations} итераций");

		// Тест lock
		object lockObj = new object();
		var lockStopwatch = System.Diagnostics.Stopwatch.StartNew();

		await Task.Run(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				lock (lockObj)
				{
					// Пустая операция
				}
			}
		});

		lockStopwatch.Stop();
		Console.WriteLine($"lock: {lockStopwatch.ElapsedMilliseconds} мс");

		// Тест Mutex (внутрипроцессный)
		var mutexStopwatch = System.Diagnostics.Stopwatch.StartNew();

		await Task.Run(() =>
		{
			using (Mutex mutex = new Mutex())
			{
				for (int i = 0; i < iterations; i++)
				{
					mutex.WaitOne();
					mutex.ReleaseMutex();
				}
			}
		});

		mutexStopwatch.Stop();
		Console.WriteLine($"Mutex: {mutexStopwatch.ElapsedMilliseconds} мс");

		if (lockStopwatch.ElapsedMilliseconds > 0)
		{
			double ratio = mutexStopwatch.ElapsedMilliseconds / (double)lockStopwatch.ElapsedMilliseconds;
			Console.WriteLine($"Mutex медленнее в {ratio:F1} раз");
		}
	}

	// 4. AbandonedMutexException
	static async Task AbandonedMutexDemo()
	{
		Console.WriteLine("Демонстрация AbandonedMutexException...");

		// Создаем и захватываем мьютекс в одном потоке
		Mutex dangerousMutex = new Mutex(false, "Global\\DangerousMutex");

		Task faultyTask = Task.Run(() =>
		{
			Console.WriteLine("Поток 1: Захватываю мьютекс");
			dangerousMutex.WaitOne();
			Console.WriteLine("Поток 1: Захватил, но не освобожу!");
			// Умышленно не вызываем ReleaseMutex
			// Объект будет утилизирован без освобождения
		});

		await faultyTask;

		// Даем время для завершения
		await Task.Delay(100);

		// Другой поток пытается захватить
		Task recoveryTask = Task.Run(() =>
		{
			Console.WriteLine("Поток 2: Пытаюсь захватить мьютекс");
			try
			{
				// Используем существующий мьютекс
				bool acquired = dangerousMutex.WaitOne(1000);
				Console.WriteLine($"Поток 2: Результат WaitOne: {acquired}");
			}
			catch (AbandonedMutexException ex)
			{
				Console.WriteLine($"Поток 2: Поймал AbandonedMutexException");
				Console.WriteLine($"  Сообщение: {ex.Message}");

				// Даже при исключении мьютекс захвачен
				dangerousMutex.ReleaseMutex();
				Console.WriteLine("Поток 2: Освободил мьютекс после исключения");
			}
			finally
			{
				dangerousMutex.Dispose();
			}
		});

		await recoveryTask;
	}

	// 5. Работа с файлом
	static async Task FileAccessExample()
	{
		Console.WriteLine("Синхронизация доступа к общему файлу...");

		// Создаем мьютекс для синхронизации доступа к файлу
		using (Mutex fileMutex = new Mutex(false, "Global\\SharedFileAccess"))
		{
			// Имитация двух независимых процессов
			Task writer1 = Task.Run(async () =>
			{
				Console.WriteLine("Писатель 1: Захватываю мьютекс");
				fileMutex.WaitOne();

				try
				{
					Console.WriteLine("Писатель 1: Начинаю запись в файл");
					await Task.Delay(800); // Имитация записи
					Console.WriteLine("Писатель 1: Запись завершена");
				}
				finally
				{
					fileMutex.ReleaseMutex();
					Console.WriteLine("Писатель 1: Освободил мьютекс");
				}
			});

			// Второй писатель запускается с задержкой
			await Task.Delay(200);

			Task writer2 = Task.Run(async () =>
			{
				Console.WriteLine("Писатель 2: Пытаюсь захватить мьютекс");

				// Ждем максимум 2 секунды
				bool acquired = fileMutex.WaitOne(2000);

				if (acquired)
				{
					try
					{
						Console.WriteLine("Писатель 2: Захватил мьютекс");
						await Task.Delay(400); // Имитация записи
						Console.WriteLine("Писатель 2: Запись завершена");
					}
					finally
					{
						fileMutex.ReleaseMutex();
						Console.WriteLine("Писатель 2: Освободил мьютекс");
					}
				}
				else
				{
					Console.WriteLine("Писатель 2: Не удалось получить доступ к файлу");
				}
			});

			await Task.WhenAll(writer1, writer2);
			Console.WriteLine("Все операции с файлом завершены");
		}
	}
}