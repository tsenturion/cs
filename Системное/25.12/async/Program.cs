using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Как работает await ===\n");

		// 1. Основное поведение await
		Console.WriteLine("1. Основное поведение await:");
		await BasicAwaitExample();

		// 2. await не блокирует поток
		Console.WriteLine("\n2. await vs Sleep (потоки):");
		await AwaitVsSleep();

		// 3. Синхронное завершение
		Console.WriteLine("\n3. Синхронное завершение:");
		await SynchronousCompletion();

		// 4. Возврат управления
		Console.WriteLine("\n4. Возврат управления:");
		await ReturnControlExample();

		// 5. Потоки после await
		Console.WriteLine("\n5. Потоки после await:");
		await ThreadsAfterAwait();

		// 6. ConfigureAwait
		Console.WriteLine("\n6. ConfigureAwait:");
		await ConfigureAwaitExample();

		// 7. await и исключения
		Console.WriteLine("\n7. await и исключения:");
		await AwaitAndExceptions();

		// 8. Несколько await подряд
		Console.WriteLine("\n8. Несколько await подряд:");
		await MultipleAwaits();

		Console.WriteLine("\n=== Ключевые выводы ===");
		Console.WriteLine("- await никогда не блокирует поток");
		Console.WriteLine("- await приостанавливает метод, а не поток");
		Console.WriteLine("- Если задача завершена, await работает синхронно");
		Console.WriteLine("- await возвращает управление вызывающему коду");
		Console.WriteLine("- await извлекает результат/исключение из Task");
	}

	// 1. Основное поведение await
	static async Task BasicAwaitExample()
	{
		Console.WriteLine($"  До await, поток: {Thread.CurrentThread.ManagedThreadId}");

		// await приостанавливает метод, а не поток
		await Task.Delay(500);

		Console.WriteLine($"  После await, поток: {Thread.CurrentThread.ManagedThreadId}");
		Console.WriteLine("  Поток мог смениться");
	}

	// 2. await не блокирует поток
	static async Task AwaitVsSleep()
	{
		Console.WriteLine($"  Начало, поток: {Thread.CurrentThread.ManagedThreadId}");

		// Создаем фоновую задачу для мониторинга
		Task monitorTask = Task.Run(() =>
		{
			for (int i = 1; i <= 5; i++)
			{
				Console.WriteLine($"    Монитор: шаг {i}, поток: {Thread.CurrentThread.ManagedThreadId}");
				Thread.Sleep(200);
			}
		});

		Console.WriteLine("  Запускаем await Task.Delay(1000)...");
		await Task.Delay(1000); // Поток освобождается

		Console.WriteLine($"  После await, поток: {Thread.CurrentThread.ManagedThreadId}");

		Console.WriteLine("\n  Теперь Sleep (для сравнения)...");
		Thread.Sleep(1000); // Поток блокируется

		Console.WriteLine($"  После Sleep, поток: {Thread.CurrentThread.ManagedThreadId}");

		await monitorTask;
	}

	// 3. Синхронное завершение
	static async Task SynchronousCompletion()
	{
		Console.WriteLine($"  Начало, поток: {Thread.CurrentThread.ManagedThreadId}");

		// Задача уже завершена - await работает синхронно
		Task<int> completedTask = Task.FromResult(42);
		Console.WriteLine($"  Состояние задачи: {completedTask.Status}");

		int result = await completedTask; // Синхронное выполнение

		Console.WriteLine($"  После await, поток: {Thread.CurrentThread.ManagedThreadId}");
		Console.WriteLine($"  Результат: {result}");

		// Сравнение с незавершенной задачей
		Console.WriteLine("\n  Незавершенная задача:");
		Task<int> delayedTask = GetDelayedValueAsync();
		Console.WriteLine($"  Состояние перед await: {delayedTask.Status}");

		result = await delayedTask; // Асинхронное выполнение
		Console.WriteLine($"  Состояние после await: {delayedTask.Status}");
	}

	static async Task<int> GetDelayedValueAsync()
	{
		await Task.Delay(300);
		return 100;
	}

	// 4. Возврат управления
	static async Task ReturnControlExample()
	{
		Console.WriteLine("  Вызываем асинхронный метод...");

		// Метод возвращает управление на первом await
		Task<int> task = ProcessAsync();
		Console.WriteLine($"  Метод вернул Task, состояние: {task.Status}");
		Console.WriteLine("  Основной код продолжает работу");

		// Делаем что-то еще
		await Task.Delay(300);
		Console.WriteLine($"  Основной код: после задержки, состояние задачи: {task.Status}");

		// Теперь ждем результат
		int result = await task;
		Console.WriteLine($"  Результат: {result}, состояние: {task.Status}");
	}

	static async Task<int> ProcessAsync()
	{
		Console.WriteLine("    ProcessAsync: Начало");
		Console.WriteLine($"    ProcessAsync: поток {Thread.CurrentThread.ManagedThreadId}");

		// Первый await - точка возврата управления
		await Task.Delay(500);

		Console.WriteLine($"    ProcessAsync: после первого await, поток {Thread.CurrentThread.ManagedThreadId}");

		// Второй await
		await Task.Delay(300);

		Console.WriteLine($"    ProcessAsync: завершение, поток {Thread.CurrentThread.ManagedThreadId}");
		return 42;
	}

	// 5. Потоки после await
	static async Task ThreadsAfterAwait()
	{
		Console.WriteLine($"  Главный поток: {Thread.CurrentThread.ManagedThreadId}");

		// В консольном приложении поток может меняться
		for (int i = 0; i < 3; i++)
		{
			await Task.Delay(100);
			Console.WriteLine($"  После await #{i + 1}: {Thread.CurrentThread.ManagedThreadId}");
		}

		// Использование Task.Run
		await Task.Run(() =>
		{
			Console.WriteLine($"  В Task.Run: {Thread.CurrentThread.ManagedThreadId}");
		});

		Console.WriteLine($"  После Task.Run: {Thread.CurrentThread.ManagedThreadId}");
	}

	// 6. ConfigureAwait
	static async Task ConfigureAwaitExample()
	{
		Console.WriteLine($"  Начало, поток: {Thread.CurrentThread.ManagedThreadId}");

		// Без ConfigureAwait - может вернуться в тот же контекст
		await Task.Delay(200);
		Console.WriteLine($"  Без ConfigureAwait: {Thread.CurrentThread.ManagedThreadId}");

		// С ConfigureAwait(false) - может продолжить в любом потоке
		await Task.Delay(200).ConfigureAwait(false);
		Console.WriteLine($"  С ConfigureAwait(false): {Thread.CurrentThread.ManagedThreadId}");

		// Еще раз для демонстрации
		await Task.Delay(200).ConfigureAwait(false);
		Console.WriteLine($"  Снова ConfigureAwait(false): {Thread.CurrentThread.ManagedThreadId}");

		Console.WriteLine("  ConfigureAwait(false) полезен в библиотеках");
	}

	// 7. await и исключения
	static async Task AwaitAndExceptions()
	{
		Console.WriteLine("  Тестирование исключений с await...");

		Task faultedTask = Task.Run(() =>
		{
			throw new InvalidOperationException("Тестовая ошибка");
		});

		// Без await исключение остается внутри Task
		Console.WriteLine($"  Состояние задачи без await: {faultedTask.Status}");
		Console.WriteLine($"  IsFaulted: {faultedTask.IsFaulted}");

		try
		{
			// await извлекает исключение
			await faultedTask;
		}
		catch (InvalidOperationException ex)
		{
			Console.WriteLine($"  Поймано исключение через await: {ex.Message}");
			Console.WriteLine($"  Состояние задачи после await: {faultedTask.Status}");
		}

		// Исключения из async метода
		try
		{
			await ThrowAsync();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Исключение из async метода: {ex.Message}");
		}
	}

	static async Task ThrowAsync()
	{
		await Task.Delay(100);
		throw new ApplicationException("Исключение из async метода");
	}

	// 8. Несколько await подряд
	static async Task MultipleAwaits()
	{
		Console.WriteLine($"  Начало, поток: {Thread.CurrentThread.ManagedThreadId}");

		// Каждый await - потенциальная точка приостановки
		Console.WriteLine("  Первый await...");
		await Task.Delay(200);
		Console.WriteLine($"  После первого await, поток: {Thread.CurrentThread.ManagedThreadId}");

		Console.WriteLine("  Второй await...");
		await Task.Delay(200);
		Console.WriteLine($"  После второго await, поток: {Thread.CurrentThread.ManagedThreadId}");

		Console.WriteLine("  Третий await...");
		await Task.Delay(200);
		Console.WriteLine($"  После третьего await, поток: {Thread.CurrentThread.ManagedThreadId}");

		// Когда все await завершены
		Console.WriteLine($"  Завершение, поток: {Thread.CurrentThread.ManagedThreadId}");
	}
}