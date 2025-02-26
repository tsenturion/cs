using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using HtmlAgilityPack;
//using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Program
{
	class Program
	{
		static async Task Main()
		{
			//string google = "google.com";
			//HttpClient client = new();
			//Console.WriteLine(await client.GetStringAsync(google + @"/search?q=yandex"));
			string query = "";
			string apiKey = "AIzaSyBrq37iNAbKkYclT26He5BGSuUz4h5el0I";
			string searchId = "";
			string apiUrl = $"https://customsearch.googleapis.com/customsearch/v1?cx={searchId}&key={apiKey}&q={query}&start=0";
			try
			{
				using (var client = new HttpClient())
				{
					var response = await client.GetAsync(apiUrl);
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