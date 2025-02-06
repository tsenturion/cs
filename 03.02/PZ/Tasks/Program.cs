using System.Runtime.Intrinsics.X86;

namespace Program
{
	class Program
	{
		static int count = 0;
		static async Task Main()
		{
			#region Task1
			//Action action = new Action(delegate 
			//{
			//	for (int i = 0; i < 100; i++)
			//	{
			//		Console.WriteLine($"\tTask2: {DateTime.Now.ToString()}");
			//		Thread.Sleep(100);
			//	}
			//});
			//Task task1 = new Task(delegate 
			//{
			//	for (int i = 0; i < 100; i++)
			//	{
			//		Console.WriteLine($"Task1: {DateTime.Now.ToString()}");
			//		Thread.Sleep(100);
			//	}
			//});
			//task1.Start();

			//Task task2 = Task.Factory.StartNew(action);

			//Task task3 = Task.Run(delegate
			//{
			//	for (int i = 0; i < 100; i++)
			//	{
			//		Console.WriteLine($"\t\tTask3: {DateTime.Now.ToString()}");
			//		Thread.Sleep(100);
			//	}
			//});

			//Console.ReadKey(); 
			#endregion

			#region Task2-3
			{
				//int start;
				//while(true)
				//{
				//	Console.WriteLine("Введите начало диапазона");
				//	try
				//	{
				//		start = Convert.ToInt32(Console.ReadLine());
				//		break;
				//	}
				//	catch (Exception e)
				//	{
				//		Console.WriteLine(e.Message);
				//	}
				//}

				//int end;
				//while (true)
				//{
				//	Console.WriteLine("Введите конец диапазона");
				//	try
				//	{
				//		end = Convert.ToInt32(Console.ReadLine());
				//		break;
				//	}
				//	catch (Exception e)
				//	{
				//		Console.WriteLine(e.Message);
				//	}
				//}

				//Task task = Task.Run(() => DisplayPrimeNumbers(start, end));

				//task.Wait();
    //            Console.WriteLine("\n" + count);

				//static void DisplayPrimeNumbers(int start, int end)
				//{
				//	for (int i = start; i <= end; i++)
				//	{
				//		if (IsPrime(i))
				//		{
				//			count++;
    //                        Console.Write(i + "\t");
				//		}
				//	}
				//}
				//static bool IsPrime(int number)
				//{
				//	if (number < 2) return false;

				//	for (int i = 2; i * i <= number; i++) if (number % i == 0) return false;

				//	return true;
				//}
			}
			#endregion

			#region Task4
			{
				Random r = new();

				int[] array = new int[100];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = r.Next(100);
					Console.Write(array[i] + "\t");
				}

				Task<int>[] tasks =
				{
					new Task<int>(() => 
					{
						int min = array[0];
						foreach(int e in array)if(min > e)min = e;
						return min;
					}),
					new Task<int>(() =>
					{
						int max = array[0];
						foreach(int e in array)if(max < e)max = e;
						return max;
					}),
					new Task<int>(() =>
					{
						int sum = 0;
						foreach(int e in array)sum += e;
						return sum / array.Length;
					}),
					new Task<int>(() =>
					{
						int sum = 0;
						foreach(int e in array)sum += e;
						return sum;
					})
				};

				Console.WriteLine("\nЗапуск задач");
				foreach (Task task in tasks) task.Start();

				Console.WriteLine("Ожидание выполнения");
				await Task.WhenAll(tasks[0], tasks[1], tasks[2], tasks[3]);
				Console.WriteLine("Все готовы");

				Console.WriteLine("Минимум: " + tasks[0].Result);
				Console.WriteLine("Максимум: " + tasks[1].Result);
				Console.WriteLine("Среднее: " + tasks[2].Result);
				Console.WriteLine("Сумма: " + tasks[3].Result);
			}
			#endregion
			//Task.WaitAll();
		}
	}
}