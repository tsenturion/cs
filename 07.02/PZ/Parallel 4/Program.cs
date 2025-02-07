namespace Program
{
	class Program
	{
		static void Main()
		{
			object lock_obj = new object();

			Random r = new();
			StreamWriter sw = new("numbers.txt");
			for(int i = 0; i < 10; i++)
			{
				sw.WriteLine(r.Next(100));
			}
			sw.Close();

			List<int> list = new List<int>();
			List<long> list_fact = new List<long>();

			StreamReader sr = new("numbers.txt");
			for (int i = 0; i < 10; i++)
			{
				list.Add(Convert.ToInt32(sr.ReadLine()));
			}
			sr.Close();

			for (int i = 0; i < 10; i++)
			{
				ThreadPool.QueueUserWorkItem(i => CalculateFactorial(list[Convert.ToInt32(i)]));
			}

			Console.ReadKey();

			long CalculateFactorial(int number)
			{
				lock (lock_obj)
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
			}

			void NumberOfNumbers(int number)
			{
				List<int> parts = new List<int>();
				int temp = number;
				while (temp > 0)
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