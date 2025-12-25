using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Как работает async ===\n");

		// 1. async без await
		Console.WriteLine("1. async без await (антипаттерн):");
		await AsyncWithoutAwait();

		// 2. Разделение метода на части
		Console.WriteLine("\n2. Разделение метода на части await:");
		await MethodPartsExample();

		// 3. Потоки в async методах
		Console.WriteLine("\n3. Потоки в async методах:");
		await ThreadsInAsync();

		// 4. Вызов async метода без ожидания
		Console.WriteLine("\n4. Вызов async метода без ожидания:");
		await FireAndForgetAsync();

		// 5. Возврат Task<T> из async метода
		Console.WriteLine("\n5. Возврат Task<T> из async метода:");
		await ReturnValueFromAsync();

		// 6. async void - опасный пример (без краха приложения)
		Console.WriteLine("\n6. async void (опасно!):");
		await DemonstrateAsyncVoid();

		// 7. Стек вызовов в async
		Console.WriteLine("\n7. Стек вызовов:");
		await CallStackDemo();

		// 8. Когда async бесполезен
		Console.WriteLine("\n8. Когда async бесполезен:");
		await UselessAsync();

		Console.WriteLine("\n=== Ключевые выводы ===");
		Console.WriteLine("- async сам по себе не делает код асинхронным");
		Console.WriteLine("- Реальная асинхронность появляется только с await");
		Console.WriteLine("- async не создает новые потоки");
		Console.WriteLine("- async void почти всегда ошибка");
		Console.WriteLine("- async != быстрее");
	}

	// 1. async без await - антипаттерн
	static async Task AsyncWithoutAwait()
	{
		Console.WriteLine($"  Выполняется в потоке: {Thread.CurrentThread.ManagedThreadId}");
		Console.WriteLine("  Этот метод async, но не содержит await");
		Console.WriteLine("  Он выполняется синхронно, Task уже завершен");
	}

	// 2. Разделение метода на части
	static async Task MethodPartsExample()
	{
		Console.WriteLine("  Часть 1: До первого await");
		Console.WriteLine($"  Поток: {Thread.CurrentThread.ManagedThreadId}");

		await Task.Delay(500);

		Console.WriteLine("  Часть 2: После первого await");
		Console.WriteLine($"  Поток: {Thread.CurrentThread.ManagedThreadId}");

		await Task.Delay(500);

		Console.WriteLine("  Часть 3: После второго await");
		Console.WriteLine($"  Поток: {Thread.CurrentThread.ManagedThreadId}");
	}

	// 3. Потоки в async методах
	static async Task ThreadsInAsync()
	{
		Console.WriteLine($"  Начало метода: {Thread.CurrentThread.ManagedThreadId}");

		await Task.Delay(100);
		Console.WriteLine($"  После Delay: {Thread.CurrentThread.ManagedThreadId}");

		Thread.Sleep(100);
		Console.WriteLine($"  После Sleep: {Thread.CurrentThread.ManagedThreadId}");

		await Task.Run(() =>
		{
			Console.WriteLine($"  В Task.Run: {Thread.CurrentThread.ManagedThreadId}");
		});

		Console.WriteLine($"  После Task.Run: {Thread.CurrentThread.ManagedThreadId}");
	}

	// 4. Вызов async метода без ожидания
	static async Task FireAndForgetAsync()
	{
		Console.WriteLine("  Запускаем задачу без ожидания...");

		Task task = ExampleAsync();
		Console.WriteLine($"  Задача запущена, состояние: {task.Status}");
		Console.WriteLine("  Основной код продолжает выполняться");

		await task;
		Console.WriteLine($"  После await, состояние: {task.Status}");
	}

	static async Task ExampleAsync()
	{
		Console.WriteLine("    ExampleAsync: Начало");
		await Task.Delay(800);
		Console.WriteLine("    ExampleAsync: Завершение");
	}

	// 5. Возврат Task<T> из async метода
	static async Task ReturnValueFromAsync()
	{
		Task<int> task1 = GetValueAsync();
		Console.WriteLine($"  Задача создана, состояние: {task1.Status}");

		int result = await task1;
		Console.WriteLine($"  Результат: {result}, состояние: {task1.Status}");

		Task<int> task2 = GetValueWithExceptionAsync();
		try
		{
			result = await task2;
		}
		catch (InvalidOperationException ex)
		{
			Console.WriteLine($"  Исключение: {ex.Message}, состояние: {task2.Status}");
		}
	}

	static async Task<int> GetValueAsync()
	{
		await Task.Delay(300);
		return 42;
	}

	static async Task<int> GetValueWithExceptionAsync()
	{
		await Task.Delay(300);
		throw new InvalidOperationException("Ошибка в async методе");
	}

	// 6. async void - безопасная демонстрация
	static async Task DemonstrateAsyncVoid()
	{
		Console.WriteLine("  Запускаем async void метод...");

		try
		{
			// Используем try-catch, но он не сработает
			AsyncVoidMethod();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Этот блок НЕ выполнится: {ex.Message}");
		}

		Console.WriteLine("  Основной поток продолжает работу...");
		Console.WriteLine("  Ожидаем, чтобы увидеть, что происходит...");

		// Ждем, чтобы async void метод успел выполниться
		await Task.Delay(500);

		Console.WriteLine("  Приложение все еще работает...");

		// Теперь покажем безопасный способ
		Console.WriteLine("\n  Правильный способ (async Task):");
		try
		{
			await AsyncTaskMethod();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Исключение поймано: {ex.Message}");
		}

		Console.WriteLine("  Демонстрация завершена успешно");
	}

	// Безопасная версия async void для демонстрации
	static async void AsyncVoidMethod()
	{
		Console.WriteLine("    AsyncVoidMethod: начало");

		try
		{
			// Вместо прямого throw, делаем это безопасно
			await Task.Delay(100);

			// Создаем исключение, но не бросаем его
			var exception = new Exception("Ошибка в async void - не ловится try-catch!");
			Console.WriteLine($"    Создано исключение: {exception.Message}");
			Console.WriteLine("    В реальном приложении это бы привело к краху!");

			// Вместо throw просто сообщаем
			Console.WriteLine("    [Здесь было бы: throw exception;]");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"    Внутренний catch: {ex.Message}");
		}
	}

	static async Task AsyncTaskMethod()
	{
		Console.WriteLine("    AsyncTaskMethod: начало");
		await Task.Delay(100);
		throw new Exception("Ошибка в async Task - ловится try-catch");
	}

	// 7. Стек вызовов
	static async Task CallStackDemo()
	{
		Console.WriteLine("  Вызываем MethodA...");
		await MethodA();
	}

	static async Task MethodA()
	{
		Console.WriteLine("    MethodA: начало");
		await Task.Delay(100);
		await MethodB();
		Console.WriteLine("    MethodA: завершение");
	}

	static async Task MethodB()
	{
		Console.WriteLine("      MethodB: начало");
		await Task.Delay(100);

		try
		{
			throw new Exception("Тест исключения");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"      StackTrace:\n{ex.StackTrace}");
		}

		Console.WriteLine("      MethodB: завершение");
	}

	// 8. Когда async бесполезен
	static async Task UselessAsync()
	{
		Console.WriteLine("  Тест CPU-bound операции:");

		var start = DateTime.Now;

		int result1 = await Task.Run(() =>
		{
			Thread.Sleep(500);
			return CalculateSomething();
		});

		var time1 = DateTime.Now - start;
		Console.WriteLine($"  Async результат: {result1}, время: {time1.TotalMilliseconds:F0}мс");

		start = DateTime.Now;
		int result2 = CalculateSomething();
		var time2 = DateTime.Now - start;
		Console.WriteLine($"  Sync результат: {result2}, время: {time2.TotalMilliseconds:F0}мс");
	}

	static int CalculateSomething()
	{
		int sum = 0;
		for (int i = 0; i < 1000000; i++)
		{
			sum += i % 100;
		}
		return sum;
	}
}