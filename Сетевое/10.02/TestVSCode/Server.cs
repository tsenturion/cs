using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TestVSCode2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Book> books = await FetchPopularBooksAsync();

            Console.WriteLine("100");
            for (int i = 0; i < books.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {books[i].Title}");
            }
            Console.WriteLine("Введите номер книги для получения текста:");
            string input = Console.ReadLine();

            while (input.ToLower() != "exit")
            {
                if (int.TryParse(input, out int bookNumber) && bookNumber > 0 && bookNumber <= books.Count)
                {
                    string bookText = await DownloadBookTextAsync(books[bookNumber - 1].Url, books[bookNumber - 1].DownloadUrl);
                    Console.Clear();
                    Console.WriteLine($"Текст {books[bookNumber - 1].Title}\n");
                    Console.WriteLine(bookText);
                }
                else
                {
                    Console.WriteLine("Неверный ввод! Введите номер книги или exit:");
                }
                Console.WriteLine("Введите номер книги для получения текста:");
                input = Console.ReadLine();
            }
        }

        private static async Task<List<Book>> FetchPopularBooksAsync()
        {
            string url = "https://www.gutenberg.org/ebooks/bookshelf/13";
            List<Book> bookList = new List<Book>();

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string html = await client.GetStringAsync(url);
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

                                string downloadUrl = $"https://www.gutenberg.org/cache/epub/{relativeUrl.Split('/')[2]}/pg{relativeUrl.Split('/')[2]}.txt"; 

                                bookList.Add(new Book { Title = title, Url = absoluteUrl, DownloadUrl = downloadUrl });
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error fetching popular books: {ex.Message}");
                }
            }
            return bookList;
        }

        private static async Task<string> DownloadBookTextAsync(string bookUrl, string downloadUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    return await client.GetStringAsync(downloadUrl);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error downloading book text: {ex.Message}");
                    return "Не удалось загрузить текст книги";
                }
            }
        }

        class Book
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string DownloadUrl { get; set; } 
        }
    }
}
