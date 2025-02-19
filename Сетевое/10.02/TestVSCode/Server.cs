using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Reflection.Metadata;
using System.Text.Encodings.Web;

/*
Создайте консольное приложение, которое позволит, 
отобразить 100 самых популярных книг из библиотеки 
Гуттенберга. По нажатию наназвание книги нужно отобразить
 текст этой книги
*/

namespace TestVSCode
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
                    string bookText = await DownloadBookTextAsync(books[bookNumber - 1].Url);
                    Console.Clear();
                    Console.WriteLine($"текст {books[bookNumber - 1].Title}\n");
                    Console.WriteLine(bookText);

                }
                else{
                    Console.WriteLine("Неверный ввод! Введите номер книги или exit:");
                }
                Console.WriteLine("Введите номер книги для получения текста:");
                input = Console.ReadLine();
            }
        }

        private static async Task<List<Book>> FetchPopularBooksAsync()
        {
            string url = "http://www.gutenberg.org/ebooks/bookshelf/13";
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
                            
                            if (linkNode!= null)
                            {
                                string title = HtmlEntity.DeEntitize(linkNode.SelectSingleNode(".//span[@class='title']").InnerText.Trim());
                                string relativeUrl = linkNode.GetAttributeValue("href", "");
                                string absoluteUrl = $"http://www.gutenberg.org{relativeUrl}";
                                bookList.Add(new Book { Title = title, Url = absoluteUrl });
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error fetching popular books: {ex.Message}");
                }
            }
            //return bookList.OrderByDescending(b => b.Title).Take(100).ToList();
            return bookList;
        }
        private static async Task<string> DownloadBookTextAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string bookText = await response.Content.ReadAsStringAsync();
                return bookText;
            }
        }

        class Book
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }
    
    }
 }