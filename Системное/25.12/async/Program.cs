using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Ограничение параллелизма ===\n");

		// 1. Проблема неограниченной параллельности
		Console.WriteLine("1. Проблема неограниченной параллельности:");
		await UnlimitedParallelismProblem();

		// 2. SemaphoreSlim - базовый пример
		Console.WriteLine("\n2. SemaphoreSlim - базовый пример:");
		await BasicSemaphoreExample();

		// 3. Сравнение производительности
		Console.WriteLine("\n3. Сравнение производительности:");
		await PerformanceComparison();

		// 4. Обработка списка с ограничением
		Console.WriteLine("\n4. Обработка списка с ограничением:");
		await ProcessListWithThrottling();

		// 5. Реальные сценарии использования
		Console.WriteLine("\n5. Реальные сценарии использования:");
		await RealWorldScenarios();

		// 6. Антипаттерны
		Console.WriteLine("\n6. Антипаттерны:");
		DemonstrateAntipatterns();

		Console.WriteLine("\n=== Ключевые выводы ===");
		Console.WriteLine("- Параллельность - ресурс, а не бонус");
		Console.WriteLine("- Бесконтрольная параллельность ведет к деградации");
		Console.WriteLine("- SemaphoreSlim - основной инструмент ограничения");
		Console.WriteLine("- try/finally обязательно при работе с семафорами");
		Console.WriteLine("- Ограниченный параллелизм - золотая середина");
	}

	// 1. Проблема неограниченной параллельности
	static async Task UnlimitedParallelismProblem()
	{
		Console.WriteLine("  Имитация 100 запросов к базе данных...");

		var stopwatch = Stopwatch.StartNew();

		try
		{
			// АНТИПАТТЕРН: запуск всего сразу
			var tasks = new List<Task>();
			for (int i = 1; i <= 100; i++)
			{
				int id = i;
				tasks.Add(Task.Run(async () =>
				{
					await SimulateDatabaseQuery(id);
				}));
			}

			await Task.WhenAll(tasks);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Ошибка: {ex.Message}");
			Console.WriteLine("  Перегружен пул соединений или исчерпаны ресурсы");
		}

		stopwatch.Stop();
		Console.WriteLine($"  Время: {stopwatch.ElapsedMilliseconds} мс");
	}

	// 2. SemaphoreSlim - базовый пример
	static async Task BasicSemaphoreExample()
	{
		// Создаем семафор на 3 одновременных операции
		SemaphoreSlim semaphore = new SemaphoreSlim(3);

		Console.WriteLine($"  Семафор создан. Доступно мест: {semaphore.CurrentCount}");

		var tasks = new List<Task>();
		for (int i = 1; i <= 10; i++)
		{
			int itemId = i;
			tasks.Add(ProcessWithSemaphoreAsync(itemId, semaphore));
		}

		await Task.WhenAll(tasks);
		Console.WriteLine("  Все задачи завершены");
	}

	static async Task ProcessWithSemaphoreAsync(int itemId, SemaphoreSlim semaphore)
	{
		Console.WriteLine($"    Элемент {itemId}: ожидает семафора");

		// Асинхронное ожидание доступа
		await semaphore.WaitAsync();

		try
		{
			Console.WriteLine($"    Элемент {itemId}: начал обработку");
			Console.WriteLine($"    Доступно мест: {semaphore.CurrentCount}");

			await Task.Delay(500); // Имитация работы

			Console.WriteLine($"    Элемент {itemId}: завершил обработку");
		}
		finally
		{
			// ВСЕГДА освобождаем семафор в finally
			semaphore.Release();
		}
	}

	// 3. Сравнение производительности
	static async Task PerformanceComparison()
	{
		const int totalOperations = 20;
		const int maxParallelism = 4;

		Console.WriteLine($"  Всего операций: {totalOperations}");
		Console.WriteLine($"  Максимальная параллельность: {maxParallelism}");

		// Тест 1: Последовательная обработка
		Console.WriteLine("\n  Тест 1: Последовательная обработка");
		var stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < totalOperations; i++)
		{
			await SimulateIoOperation(i);
		}
		stopwatch.Stop();
		Console.WriteLine($"  Время: {stopwatch.ElapsedMilliseconds} мс");

		// Тест 2: Без ограничений
		Console.WriteLine("\n  Тест 2: Без ограничений");
		stopwatch.Restart();
		var tasks = new List<Task>();
		for (int i = 0; i < totalOperations; i++)
		{
			int id = i;
			tasks.Add(Task.Run(() => SimulateIoOperation(id)));
		}
		await Task.WhenAll(tasks);
		stopwatch.Stop();
		Console.WriteLine($"  Время: {stopwatch.ElapsedMilliseconds} мс");

		// Тест 3: Ограниченная параллельность
		Console.WriteLine("\n  Тест 3: Ограниченная параллельность");
		var semaphore = new SemaphoreSlim(maxParallelism);
		stopwatch.Restart();

		tasks.Clear();
		for (int i = 0; i < totalOperations; i++)
		{
			int id = i;
			tasks.Add(ProcessWithThrottlingAsync(id, semaphore));
		}

		await Task.WhenAll(tasks);
		stopwatch.Stop();
		Console.WriteLine($"  Время: {stopwatch.ElapsedMilliseconds} мс");
	}

	static async Task SimulateIoOperation(int id)
	{
		await Task.Delay(300); // Имитация I/O операции
	}

	static async Task ProcessWithThrottlingAsync(int id, SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		try
		{
			await SimulateIoOperation(id);
		}
		finally
		{
			semaphore.Release();
		}
	}

	// 4. Обработка списка с ограничением
	static async Task ProcessListWithThrottling()
	{
		var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		Console.WriteLine($"  Обработка {items.Count} элементов");

		// Создаем семафор для ограничения параллельности
		SemaphoreSlim semaphore = new SemaphoreSlim(2);

		var tasks = new List<Task<string>>();
		foreach (var item in items)
		{
			tasks.Add(ProcessItemAsync(item, semaphore));
		}

		var results = await Task.WhenAll(tasks);

		Console.WriteLine("  Результаты:");
		foreach (var result in results)
		{
			Console.WriteLine($"    {result}");
		}
	}

	static async Task<string> ProcessItemAsync(int item, SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		try
		{
			await Task.Delay(300); // Имитация работы
			return $"Элемент {item} обработан";
		}
		finally
		{
			semaphore.Release();
		}
	}

	// 5. Реальные сценарии использования
	static async Task RealWorldScenarios()
	{
		Console.WriteLine("  Сценарий 1: Запросы к внешнему API");
		await ApiRateLimitingExample();

		Console.WriteLine("\n  Сценарий 2: Работа с файлами");
		await FileOperationsExample();
	}

	static async Task ApiRateLimitingExample()
	{
		// Ограничение: не более 3 запросов в секунду к внешнему API
		SemaphoreSlim apiSemaphore = new SemaphoreSlim(3);

		var apiTasks = new List<Task>();
		for (int i = 1; i <= 10; i++)
		{
			int requestId = i;
			apiTasks.Add(MakeApiRequestAsync(requestId, apiSemaphore));
		}

		await Task.WhenAll(apiTasks);
		Console.WriteLine("    Все API запросы завершены");
	}

	static async Task MakeApiRequestAsync(int requestId, SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		try
		{
			Console.WriteLine($"    API запрос #{requestId} начат");
			await Task.Delay(500); // Имитация API вызова
			Console.WriteLine($"    API запрос #{requestId} завершен");
		}
		finally
		{
			semaphore.Release();
		}
	}

	static async Task FileOperationsExample()
	{
		// Ограничение одновременных операций с файлами
		SemaphoreSlim fileSemaphore = new SemaphoreSlim(2);

		var fileTasks = new List<Task>();
		for (int i = 1; i <= 5; i++)
		{
			int fileId = i;
			fileTasks.Add(WriteToFileAsync(fileId, fileSemaphore));
		}

		await Task.WhenAll(fileTasks);
		Console.WriteLine("    Все файловые операции завершены");
	}

	static async Task WriteToFileAsync(int fileId, SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		try
		{
			Console.WriteLine($"    Запись в файл #{fileId} начата");
			await Task.Delay(400); // Имитация записи
			Console.WriteLine($"    Запись в файл #{fileId} завершена");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// 6. Антипаттерны
	static void DemonstrateAntipatterns()
	{
		Console.WriteLine("  Антипаттерн 1: Забытый Release");
		Console.WriteLine("    Без finally ресурсы могут быть заблокированы навсегда");

		Console.WriteLine("\n  Антипаттерн 2: Task.Run + Semaphore без необходимости");
		Console.WriteLine("    Лишняя обертка в Task.Run усложняет код");

		Console.WriteLine("\n  Антипаттерн 3: Глобальный семафор без Dispose");
		Console.WriteLine("    Семафоры реализуют IDisposable - нужно освобождать ресурсы");

		Console.WriteLine("\n  Антипаттерн 4: Слишком низкие лимиты");
		Console.WriteLine("    Слишком строгие ограничения могут снизить производительность");

		// Пример правильного Dispose
		Console.WriteLine("\n  Правильное использование:");
		using (var semaphore = new SemaphoreSlim(2))
		{
			Console.WriteLine("    Семафор создан в using блоке");
		}
		Console.WriteLine("    Ресурсы освобождены автоматически");
	}

	static async Task SimulateDatabaseQuery(int id)
	{
		await Task.Delay(100); // Имитация запроса к БД
	}
}