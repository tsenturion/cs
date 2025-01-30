namespace Casino
{
	class Casino
	{
		class Player
		{
			static Random rand;
			public int Money { get; set; }
			Thread thread;
			public Player()
			{
				Money = 1000;
				rand = new Random((int)DateTime.Now.Ticks);
				thread = new Thread(PlaceBet);
				thread.
			}
			public int[] PlaceBet()
			{
				if (Money == 0) return [0, 0];
				return [rand.Next(0, 33), rand.Next(1, Money)];
			}
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
				for (int i = 0; i < PlayersCount; i++)
				{
					int[] PlayerBet = Players[i].PlaceBet();
					Console.WriteLine($"Player{i} bet: {PlayerBet[0]}. Money: {PlayerBet[1]}");
					if (PlayerBet[0] == WinNumber)
					{
						Players[i].Money += PlayerBet[1] * 2;
						Console.WriteLine($"Player{i} won! His bank is: {Players[i].Money}");
					}
					else
					{
						Players[i].Money -= PlayerBet[1];
						Console.WriteLine($"Player{i} lost! His bank is: {Players[i].Money}");
					}
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