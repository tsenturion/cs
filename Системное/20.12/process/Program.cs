using System;
using System.Threading.Tasks;

class Program
{
	static void Main()
	{
		// Создание и запуск задачи с помощью Task.Run
		Task task = Task.Run(() =>
		{
			Console.WriteLine("Задача выполняется");

			// Добавляем имитацию работы в задаче
			SimulateTaskWork();
		});

		// Ожидание завершения задачи (блокирует текущий поток)
		task.Wait();

		Console.WriteLine("Задача завершена");

		// Дополнительная информация о задаче (без изменения структуры)
		DisplayTaskInfo(task);

		// Демонстрация других возможностей Task API
		DemonstrateTaskFeatures();
	}

	static void SimulateTaskWork()
	{
		// Имитация работы в задаче
		Console.WriteLine("Задача: Начало обработки...");

		for (int i = 1; i <= 3; i++)
		{
			Console.WriteLine($"  Задача: шаг {i}/3 выполнен");
			Task.Delay(300).Wait(); // Имитация асинхронной задержки
		}

		Console.WriteLine("Задача: Обработка завершена");
	}

	static void DisplayTaskInfo(Task completedTask)
	{
		Console.WriteLine("\n=== Информация о задаче ===");
		Console.WriteLine($"ID задачи: {completedTask.Id}");
		Console.WriteLine($"Статус задачи: {completedTask.Status}");
		Console.WriteLine($"Завершена успешно: {completedTask.IsCompletedSuccessfully}");
		Console.WriteLine($"Отменена: {completedTask.IsCanceled}");
		Console.WriteLine($"Завершена с ошибкой: {completedTask.IsFaulted}");

		// Показываем исключение если было
		if (completedTask.IsFaulted && completedTask.Exception != null)
		{
			Console.WriteLine($"Исключение: {completedTask.Exception.Message}");
		}
	}

	static void DemonstrateTaskFeatures()
	{
		Console.WriteLine("\n=== Демонстрация возможностей Task ===");

		// 1. Возвращающая значение задача
		Console.WriteLine("\n1. Задача с возвращаемым значением:");
		Task<int> valueTask = Task.Run(() =>
		{
			Console.WriteLine("  Вычисление значения...");
			return 42;
		});

		valueTask.Wait();
		Console.WriteLine($"  Результат: {valueTask.Result}");

		// 2. Асинхронное ожидание с продолжением (ContinueWith)
		Console.WriteLine("\n2. Цепочка задач (ContinueWith):");
		Task continuationTask = Task.Run(() =>
		{
			Console.WriteLine("  Первая задача выполняется");
			return "Результат первой задачи";
		})
		.ContinueWith(previousTask =>
		{
			Console.WriteLine($"  Вторая задача получает: {previousTask.Result}");
			Console.WriteLine("  Вторая задача выполняется");
		});

		continuationTask.Wait();

		// 3. Параллельное выполнение нескольких задач
		Console.WriteLine("\n3. Параллельное выполнение задач:");
		Task[] parallelTasks = new Task[3];

		for (int i = 0; i < parallelTasks.Length; i++)
		{
			int taskNumber = i + 1;
			parallelTasks[i] = Task.Run(() =>
			{
				Console.WriteLine($"  Задача {taskNumber} запущена");
				Task.Delay(100 * taskNumber).Wait();
				Console.WriteLine($"  Задача {taskNumber} завершена");
			});
		}

		// Ожидание всех задач
		Task.WaitAll(parallelTasks);
		Console.WriteLine("  Все параллельные задачи завершены");

		// 4. Отмена задачи с CancellationToken
		Console.WriteLine("\n4. Задача с возможностью отмены:");
		var cancellationSource = new System.Threading.CancellationTokenSource();

		Task cancellableTask = Task.Run(() =>
		{
			Console.WriteLine("  Задача с отменой запущена");

			for (int i = 0; i < 10; i++)
			{
				if (cancellationSource.Token.IsCancellationRequested)
				{
					Console.WriteLine("  Задача отменена");
					cancellationSource.Token.ThrowIfCancellationRequested();
				}

				Console.WriteLine($"  Шаг {i + 1}/10");
				Task.Delay(100).Wait();
			}
		}, cancellationSource.Token);

		// Симулируем отмену через некоторое время
		Task.Run(() =>
		{
			Task.Delay(300).Wait();
			Console.WriteLine("  Инициируем отмену...");
			cancellationSource.Cancel();
		});

		try
		{
			cancellableTask.Wait();
		}
		catch (AggregateException ex)
		{
			Console.WriteLine($"  Поймано исключение: {ex.InnerExceptions[0].GetType().Name}");
		}

		// 5. Использование Task.Delay для асинхронных задержек
		Console.WriteLine("\n5. Асинхронная задержка (Task.Delay):");
		Console.WriteLine("  Начало ожидания...");
		Task.Delay(500).Wait();
		Console.WriteLine("  Ожидание завершено");
	}
}