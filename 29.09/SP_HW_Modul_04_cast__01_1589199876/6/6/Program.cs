namespace Casino
{
	class Casino
	{
		class Player
		{
			static Random rand;
			//static int objectCount = 0;
			public static int ObjectCount { get; set; }
			public int ID { get; set; }
			//public string Name { get; set; }
			public int Money { get; set; }
			private int betNumber;
			public int BetNumber 
			{
				get => betNumber;
				set
				{
					if (value >= 0 && value <= 32) betNumber = value;
				}
			}
			private int betMoney;
			public int BetMoney 
			{ 
				get => betMoney;
				set => betMoney = value > 0 ? value : 0;
			}
			public bool IsActive { get; set; }
			//Thread thread;
			public Player()
			{
				Money = 1000;
				rand = new Random((int)DateTime.Now.Ticks);
				IsActive = false;
				ObjectCount++;
				ID = ObjectCount;
				//thread = new Thread(PlaceBet);
				//thread.
			}
			~Player() { }
			public void PlaceBet(object a)
			{
				IsActive = false;
				//Thread.Sleep(rand.Next(100, 500));
				BetNumber = rand.Next(0, 33);
				BetMoney = rand.Next(100, 1000);
				IsActive = true;
			}
			//public int[] PlaceBet()
			//{
			//	if (Money == 0) return [0, 0];
			//	return [rand.Next(0, 33), rand.Next(1, Money)];
			//}
		}

		class RouletteTable
		{
			Random rand = new Random((int)DateTime.Now.Ticks);
			int TablePlayersCount { get; set; }
			int dayPlayersCount;
			public int DayPlayersCount { get; set; }
			Player[] Players = null;
			int roundCount = 0;
			public RouletteTable()
			{
				TablePlayersCount = 5;
				Players = new Player[TablePlayersCount];
				for (int i = 0; i < TablePlayersCount; i++)
					Players[i] = new Player();
				DayPlayersCount = rand.Next(20, 101);
			}
			public void PlayRound()
			{
				StreamWriter sr = new StreamWriter($"PlayDay.txt", true);
				int WinNumber = rand.Next(0, 33);
				Console.WriteLine($"WinNumber: {WinNumber}");

				//foreach(Player player in Players)
				//while(dayPlayersCount > Player.ObjectCount)
				//{
					for (int i = 0; i < TablePlayersCount; i++)
					{
						for (int j = 0; j < TablePlayersCount; j++)ThreadPool.QueueUserWorkItem(Players[j].PlaceBet);
						//int[] PlayerBet = Players[i].PlaceBet();
						if (Players[i].Money > 0)
						{
							while (!Players[i].IsActive) Thread.Sleep(20);
							Console.WriteLine($"Player{Players[i].ID} bet: {Players[i].BetNumber}. Money: {Players[i].BetMoney}");
							if (Players[i].BetNumber == WinNumber)
							{
								Players[i].Money += Players[i].BetMoney * 2;
								Console.WriteLine($"Player{Players[i].ID} won! His bank is: {Players[i].Money}");
							}
							else
							{
								Players[i].Money -= Players[i].BetMoney;
								Console.WriteLine($"Player{Players[i].ID} lost! His bank is: {Players[i].Money}");
							}
						}
						else
						{
						//записывать начальную сумму
							sr.WriteLine($"Игрок {Players[i].ID}: Было 1000$, стало {Players[i].Money}");
							Players[i] = new Player();
						}
					}
					roundCount++;
				//}
				sr.Close();
			}
		}
		static void Main()
		{
			RouletteTable table = new();
			Console.WriteLine($"Сегодня играют {table.DayPlayersCount} человек");
			while(table.DayPlayersCount > Player.ObjectCount)table.PlayRound();
			Console.WriteLine("День окончен! Всем спасибо за денежки");
		}
	}
}