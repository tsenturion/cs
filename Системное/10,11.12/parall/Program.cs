using System;
using System.Collections.Generic;

class Program
{
	static void Main()
	{
		// ШАГ 1. Подготовка данных
		// Создаём коллекцию чисел от 1 до 10 000 000.
		// Пока мы просто подготавливаем данные, без вычислений.
		List<int> numbers = new List<int>(10_000_000);

		for (int i = 1; i <= 10_000_000; i++)
		{
			numbers.Add(i);
		}

		// Проверка, что данные действительно созданы
		Console.WriteLine($"Количество элементов: {numbers.Count}");
		Console.WriteLine("Подготовка данных завершена.");
	}

	// ШАГ 1. Метод тяжёлого вычисления (CPU-bound)
	static int HeavyCalculation(int x)
	{
		// Локальная переменная, не разделяется между потоками
		int result = 0;

		// Искусственно нагружаем процессор
		for (int i = 0; i < 100; i++)
		{
			result += (x * i) % 7;
		}

		// Возвращаем результат вычисления
		return result;
	}
}
