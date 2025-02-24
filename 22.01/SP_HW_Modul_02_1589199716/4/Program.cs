using System.Diagnostics;

namespace Program
{
	class Program
	{
		static void Main()
		{
			string path = "";
			while(true)
			{
				Console.WriteLine("Задайте путь к файлу: ");
				path = Console.ReadLine();
				if (Path.Exists(path)) break;
				else Console.WriteLine("Некорректный путь");
			}
			Console.WriteLine("Задайте ключевое слово: ");
			string userWord = Console.ReadLine();
			StreamReader sr = new(path);
			string[] text = sr.ReadToEnd().Split([' ', ',', '!', '?', '.', ';']);
			int wordsCount = 0;
			foreach(string word in text)
				if (word == userWord) wordsCount++;
			Console.WriteLine("Таких слов " + wordsCount);
		}
	}
}