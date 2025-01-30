namespace Casino
{
	class Casino
	{
		class Player
		{
			static Random rand;
			static int objectCount = 0;
			public int Index { get; set; }
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
				objectCount++;
				Index = objectCount;
				//thread = new Thread(PlaceBet);
				//thread.
			}
			public void PlaceBet(object a)
			{
				IsActive = false;
				Thread.Sleep(rand.Next(100, 500));
				BetNumber = rand.Next(0, 33);
				BetMoney = rand.Next(1, 1000);
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
			int PlayersCount { get; set; }
			Player[] Players = null;
			public RouletteTable()
			{
				PlayersCount = 5;
				Players = new Player[PlayersCount];
				for (int i = 0; i < PlayersCount; i++)
					Players[i] = new Player();
			}
			public void PlayRound()
			{
				int WinNumber = rand.Next(0, 33);
				Console.WriteLine($"WinNumber: {WinNumber}");

				//foreach
				//Player{player.Index} bet
				for (int i = 0; i < PlayersCount; i++)
				{
					for (int j = 0; j < PlayersCount; j++)ThreadPool.QueueUserWorkItem(Players[i].PlaceBet);
					//int[] PlayerBet = Players[i].PlaceBet();
					while (!Players[i].IsActive) Thread.Sleep(20);
					Console.WriteLine($"Player{i} bet: {Players[i].BetNumber}. Money: {Players[i].BetMoney}");
					if (Players[i].BetNumber == WinNumber)
					{
						Players[i].Money += Players[i].BetMoney * 2;
						Console.WriteLine($"Player{i} won! His bank is: {Players[i].Money}");
					}
					else
					{
						Players[i].Money -= Players[i].BetMoney;
						Console.WriteLine($"Player{i} lost! His bank is: {Players[i].Money}");
					}

					//int[] PlayerBet = Players[i].PlaceBet();
					//Console.WriteLine($"Player{i} bet: {PlayerBet[0]}. Money: {PlayerBet[1]}");
					//if (PlayerBet[0] == WinNumber)
					//{
					//	Players[i].Money += PlayerBet[1] * 2;
					//	Console.WriteLine($"Player{i} won! His bank is: {Players[i].Money}");
					//}
					//else
					//{
					//	Players[i].Money -= PlayerBet[1];
					//	Console.WriteLine($"Player{i} lost! His bank is: {Players[i].Money}");
					//}
				}
			}
		}
		static void Main()
		{
			RouletteTable table = new();
			table.PlayRound();
		}
	}
}