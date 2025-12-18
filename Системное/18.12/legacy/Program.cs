using System;
using System.Runtime.InteropServices;

namespace LegacyInteropExample
{
	public static class LegacyMath
	{
		[DllImport("legacy_math.dll")]
		public static extern int add(int a, int b); // Импорт функции add из неуправляемой библиотеки
	}

	// Главный класс программы с точкой входа
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				// Вызов неуправляемой функции add из legacy_math.dll
				int result = LegacyMath.add(7, 4);

				// Вывод результата вызова неуправляемой функции
				Console.WriteLine($"Результат вызова legacy_math.dll: {result}");
			}
			catch (DllNotFoundException ex)
			{
				// Обработка ошибки если библиотека не найдена
				Console.WriteLine($"Ошибка: Не удалось найти библиотеку legacy_math.dll");
				Console.WriteLine($"Подробности: {ex.Message}");
			}
			catch (Exception ex)
			{
				// Обработка других возможных ошибок
				Console.WriteLine($"Ошибка при вызове неуправляемого кода: {ex.Message}");
			}
		}
	}
}