namespace Program
{
	class Program
	{
		static void Main()
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

			#region Task2
			//{
			//	Task task = Task.Run(delegate
			//	{
			//		for(int i=2;i<1000;i++)
			//		{
			//			if(i % i + 1 != 0 && i % i + 2 != 0&& i % i + 3 != 0)
			//			Console.WriteLine(i + "\t");
			//		}
			//	});
			//}
			//Console.ReadKey();
			#endregion

			#region Task4
			{

			}
			#endregion
			//Task.WaitAll();
		}
	}
}