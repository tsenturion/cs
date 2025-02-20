using HtmlAgilityPack;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;
//using HtmlAgilityPack;

namespace Program
{
	class Program
	{
		class Book
		{
			public string Title { get; set; }
			public string URL { get; set; }
		}
		static async Task Main()
		{
			List<Book> books = await FetchPopularBooksAsync();
			Console.WriteLine("100");
			for (int i = 0; i < books.Count; i++)
			{
				Console.WriteLine($"{i + 1}: {books[i].Title}");
			}
			Console.WriteLine("Введите номер книги для получения текста");
			string input = Console.ReadLine();
			while(input.ToLower() != "exit")
			{
				if(int.TryParse(input, out int bookNumber) && bookNumber > 0 && bookNumber <= books.Count)
				{
					string bookText = await DownloadBookTextAsync(books[bookNumber - 1].URL);
					Console.Clear();
					Console.WriteLine($"Текст книги {books[bookNumber - 1].Title}:\n");
					Console.WriteLine(bookText);
				}
				else { Console.WriteLine("некорректный ввод. можно exit"); }
				input = Console.ReadLine();
			}
			//Console.WriteLine(top100books);
		}
		static async Task<List<Book>> FetchPopularBooksAsync()
		{
			string URL = "https://www.gutenberg.org/ebooks/bookshelf/13";
			List<Book> bookList = new();
			using(HttpClient Client = new())
			{
				try
				{
					string html = await Client.GetStringAsync(URL);
					HtmlDocument document = new HtmlDocument();
					document.LoadHtml(html);

					var bookNodes = document.DocumentNode.SelectNodes("//li[@class='booklink']");
					if (bookNodes != null)
					{
						foreach (var bookNode in bookNodes)
						{
							var linkNode = bookNode.SelectSingleNode(".//a[@class='link']");
							if (linkNode != null)
							{
								string title = HtmlEntity.DeEntitize(linkNode.SelectSingleNode(".//span[@class='title']").InnerText.Trim());
								string relativeUrl = linkNode.GetAttributeValue("href", "");
								string absoluteUrl = $"https://www.gutenberg.org{relativeUrl}";
								bookList.Add(new Book { Title = title, URL = absoluteUrl });
							}
						}
					}
				}
				catch (HttpRequestException e)
				{
					Console.WriteLine("ашипка " + e.Message);
				}
			}
			return bookList;
		}
		static async Task<string> DownloadBookTextAsync(string Url)
		{
			using (HttpClient client = new())
			{
				Uri ub = new(Url);
				HttpResponseMessage response = await client.GetAsync(ub);
				string text = await response.Content.ReadAsStringAsync();
				return text;
			}

		}
	}
}