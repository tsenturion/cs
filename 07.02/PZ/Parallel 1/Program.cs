namespace Program
{
	class Program
	{
		static void Main()
		{
			int number = 234;
			Parallel.Invoke(
				() => CalculateFactorial(number),
				() => NumberOfNumbers(number)
				);

			long CalculateFactorial(int number)
			{
				long[] particalFactorials = new long[number];

				for (int i = 0; i < particalFactorials.Length; i++)
				{
					particalFactorials[i] = 1;
				}

				Parallel.For(1, number + 1, i =>
				{
					long factorial = 1;
					for (int j = 1; j <= i; j++)
					{
						factorial += j;
					}

					particalFactorials[i - 1] = factorial;
				});

				long result = 1;
				for (int i = 0; i < particalFactorials.Length; i++)
				{
					result += particalFactorials[i];
				}

				Console.WriteLine($"факториал числа {number} равен {result}");

				return result;
			}

			void NumberOfNumbers(int number)
			{
				List<int> parts = new List<int>();
				int temp = number;
				while(temp > 0)
				{
					parts.Add(temp % 10);
					//Console.Write(temp % 10);
					temp /= 10;
				}
				Console.WriteLine($"Количество цифр числа: {parts.Count}");
				Console.WriteLine($"Сумма цифр числа: {parts.Sum()}");
			}
		}
	}
}