using System;

namespace ManagedExample
{
	public class Calculator
	{
		public int Add(int a, int b)
		{
			return a + b; // Метод сложения двух целых чисел
		}
	}

	// Главный класс программы с точкой входа
	class Program
	{
		static void Main(string[] args)
		{
			// Создаем экземпляр класса Calculator
			Calculator calc = new Calculator();

			// Вызываем метод Add с аргументами 5 и 3
			int result = calc.Add(5, 3);

			// Выводим результат сложения в консоль
			Console.WriteLine($"Результат сложения: {result}");
		}
	}
}