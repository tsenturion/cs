using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestVSCode
{
	class Program
	{
		/*
        Создайте приложение, которое позволит искать ин формацию в поисковой системе. 
        Пользователь вводит текст для поиска, приложение отображает информацию 
        с первой страницы результатов поиска. В качестве поис ковой системы можно 
        использовать любую систему на ваш выбор. Например, Google или Bing
        */
		static async Task Main(string[] args)
		{
			Console.WriteLine("Enter search query:");
			string query = Console.ReadLine();
			string apiKey = "f360ac11";
			string searchId = "";
			string apiUrl = $"http://www.omdbapi.com/?apikey=[{apiKey}]";
			//string apiUrl = $"http://www.omdbapi.com/?i=tt3896198&apikey=66d636eb&{query}";
			try
			{
				using (var client = new HttpClient())
				{
					var response = await client.GetAsync($"{apiUrl}?t={query}&apikey={apiKey}");
					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						JObject jResponse = JObject.Parse(content);
						foreach (var result in jResponse["items"])
						{
							Console.WriteLine($"Title: {result["title"]}");
							Console.WriteLine($"Link: {result["link"]}");
							Console.WriteLine($"Snippet: {result["snippet"]}");
							Console.WriteLine();
						}
					}
					else
					{
						Console.WriteLine($"Error: {response.StatusCode}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}
}