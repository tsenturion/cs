using System.Net.Http;

namespace Program
{
	class Program
	{
		static async Task Main()
		{
			Console.WriteLine("Загрузка...");
			string hamletText = await DownloadHamletText();
			Console.Clear();
			Console.WriteLine(hamletText);
			Console.ReadKey();
			Console.Clear();
		}
		static async Task<string> DownloadHamletText()
		{
			string hamletURL = "https://www.gutenberg.org/files/1524/1524-0.txt";
			using (HttpClient client = new())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(hamletURL);
					response.EnsureSuccessStatusCode();
					string hamletText = await response.Content.ReadAsStringAsync();
					return hamletText;
				}
				catch(HttpRequestException e)
				{
					Console.WriteLine("Ошибка: " + e.Message);
					return "Не удалось";
				}
			}
		}
	}
}