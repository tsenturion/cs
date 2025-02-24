using HtmlAgilityPack;
using System.Net.Http;

namespace Program
{
	class Program
	{
		static async Task Main()
		{
			HttpClient client = new();
			HttpResponseMessage response = await client.GetAsync("https://www.gutenberg.org/ebooks/75453");
			HtmlDocument document = new();
			document.LoadHtml(await response.Content.ReadAsStringAsync());
			//document.DocumentNode
			
			foreach (var node in document.DocumentNode.SelectNodes("//a[@class='link']"))
			{
				string txt_url = node.GetAttributeValue("href", "");
				if (txt_url.Split(".").Last() == "txt" || txt_url.Split(".").Last() == "utf-8")
				{
					string absoluteUrl = "https://www.gutenberg.org" + txt_url;
					response = await client.GetAsync(absoluteUrl);
					Console.Write(await response.Content.ReadAsStringAsync());
				}
			}
					//Console.Write(document.DocumentNode);
		}
	}
}