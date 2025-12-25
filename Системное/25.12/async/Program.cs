using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("=== Task<T> и возврат значений ===\n");

		// 1. Возврат значений через Task<T>
		Console.WriteLine("1. Возврат значений через Task<T>:");
		await TaskReturnExample();

		// 2. await извлекает результат
		Console.WriteLine("\n2. await извлекает результат:");
		await AwaitExtractsValue();

		// 3. Исключения в Task<T>
		Console.WriteLine("\n3. Исключения в Task<T>:");
		await ExceptionsInTasks();

		// 4. AggregateException vs обычные исключения
		Console.WriteLine("\n4. AggregateException:");
		await AggregateExceptionExample();

		// 5. Task.WhenAll с исключениями
		Console.WriteLine("\n5. Task.WhenAll с несколькими исключениями:");
		await MultipleExceptions();

		// 6. Обработка исключений внутри метода
		Console.WriteLine("\n6. Обработка исключений внутри метода:");
		await InternalExceptionHandling();

		// 7. Опасность забытых задач
		Console.WriteLine("\n7. Забытые задачи (Fire and Forget):");
		DemonstrateForgottenTask();

		Console.WriteLine("\n=== Ключевые выводы ===");
		Console.WriteLine("- Task<T> - обещание будущего результата");
		Console.WriteLine("- await извлекает результат/исключения");
		Console.WriteLine("- Избегайте .Result (блокировка и AggregateException)");
		Console.WriteLine("- Всегда обрабатывайте или ожидайте задачи");
	}

	static async Task TaskReturnExample()
	{
		Task<int> calculationTask = CalculateAsync();
		Console.WriteLine($"  Задача создана, состояние: {calculationTask.Status}");

		// await извлекает результат
		int result = await calculationTask;
		Console.WriteLine($"  Результат: {result}");
		Console.WriteLine($"  Состояние: {calculationTask.Status}");
	}

	static Task<int> CalculateAsync()
	{
		return Task.Run(() =>
		{
			Thread.Sleep(500);
			return 10 + 20; // 30
		});
	}

	static async Task AwaitExtractsValue()
	{
		Task<int> task1 = Task.FromResult(42);
		Task<int> task2 = Task.Run(() => 100);

		int result1 = await task1;
		int result2 = await task2;

		Console.WriteLine($"  FromResult: {result1}");
		Console.WriteLine($"  Task.Run: {result2}");
		Console.WriteLine($"  Сумма: {result1 + result2}");
	}

	static async Task ExceptionsInTasks()
	{
		Task<int> faultedTask = FailAsync();
		Console.WriteLine($"  Задача создана, состояние: {faultedTask.Status}");
		Console.WriteLine($"  IsFaulted: {faultedTask.IsFaulted}");

		try
		{
			// Исключение проявится только здесь
			int result = await faultedTask;
		}
		catch (InvalidOperationException ex)
		{
			Console.WriteLine($"  Поймано: {ex.Message}");
			Console.WriteLine($"  Состояние после await: {faultedTask.Status}");
			Console.WriteLine($"  IsFaulted: {faultedTask.IsFaulted}");
		}
	}

	static Task<int> FailAsync()
	{
		// Используем локальную функцию для явного указания типа
		static int ThrowException()
		{
			throw new InvalidOperationException("Ошибка вычисления");
		}

		return Task.Run((Func<int>)ThrowException);
	}

	static async Task AggregateExceptionExample()
	{
		// Используем локальную функцию для явного указания типа
		static int ThrowDivideByZero()
		{
			throw new DivideByZeroException("Деление на ноль");
		}

		Task<int> task = Task.Run((Func<int>)ThrowDivideByZero);

		// Даем задаче завершиться
		await Task.Delay(100);

		try
		{
			// .Result выбрасывает AggregateException
			int result = task.Result;
		}
		catch (AggregateException aex)
		{
			Console.WriteLine($"  Пойман AggregateException (через .Result)");
			Console.WriteLine($"  Внутреннее исключение: {aex.InnerExceptions[0].GetType().Name}");
			Console.WriteLine($"  Сообщение: {aex.InnerExceptions[0].Message}");
		}

		// Создаем новую задачу для демонстрации await
		static int ThrowDivideByZero2()
		{
			throw new DivideByZeroException("Деление на ноль 2");
		}

		Task<int> task2 = Task.Run((Func<int>)ThrowDivideByZero2);

		// Сравнение с await
		try
		{
			int result = await task2; // Выбросит DivideByZeroException, а не AggregateException
		}
		catch (DivideByZeroException ex)
		{
			Console.WriteLine($"  Пойман DivideByZeroException (через await): {ex.Message}");
		}
	}

	static async Task MultipleExceptions()
	{
		Task task1 = Task.Run(() => throw new Exception("Ошибка 1"));
		Task task2 = Task.Run(() => throw new Exception("Ошибка 2"));
		Task task3 = Task.Run(() => throw new Exception("Ошибка 3"));

		Task allTasks = Task.WhenAll(task1, task2, task3);

		try
		{
			await allTasks;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Первое исключение: {ex.Message}");

			// Для WhenAll исключения находятся в AggregateException
			if (allTasks.Exception is AggregateException aex)
			{
				Console.WriteLine($"  Всего исключений: {aex.InnerExceptions.Count}");
				foreach (var inner in aex.InnerExceptions)
				{
					Console.WriteLine($"    - {inner.Message}");
				}
			}

			// Проверяем исходные задачи
			Console.WriteLine($"  task1.IsFaulted: {task1.IsFaulted}");
			Console.WriteLine($"  task2.IsFaulted: {task2.IsFaulted}");
			Console.WriteLine($"  task3.IsFaulted: {task3.IsFaulted}");
		}
	}

	static async Task InternalExceptionHandling()
	{
		Task<int> safeTask = SafeCalculateAsync(true);
		Task<int> unsafeTask = SafeCalculateAsync(false);

		try
		{
			int result1 = await safeTask;
			Console.WriteLine($"  Безопасный результат: {result1}");

			int result2 = await unsafeTask;
			Console.WriteLine($"  Небезопасный результат: {result2}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"  Исключение снаружи: {ex.Message}");
		}
	}

	static async Task<int> SafeCalculateAsync(bool handleInternally)
	{
		if (handleInternally)
		{
			try
			{
				await Task.Delay(200);
				throw new Exception("Внутренняя ошибка");
			}
			catch
			{
				return -1; // Подавляем исключение
			}
		}
		else
		{
			await Task.Delay(200);
			throw new Exception("Необработанная ошибка");
		}
	}

	static void DemonstrateForgottenTask()
	{
		Console.WriteLine("  Запускаем забытую задачу...");

		// АНТИПАТТЕРН - задача может "потеряться"
		Task.Run(() =>
		{
			Thread.Sleep(500);
			throw new Exception("Потерянная ошибка");
		});

		Console.WriteLine("  Задача запущена, но не ожидается");
		Console.WriteLine("  Исключение может проявиться в неожиданный момент");

		// Даем время для демонстрации (обычно так не делают)
		Thread.Sleep(1000);

		Console.WriteLine("  Приложение продолжает работу");
	}
}