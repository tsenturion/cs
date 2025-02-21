using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Web;

namespace TestVSCode
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string gutenbergBaseUrl = "https://www.gutenberg.org";
        static async Task Main(string[] args)
        {
            Console.WriteLine("введите фамилию имя для поиска");
            string authorName = Console.ReadLine();
            await SearchAndDownloadBooks(authorName);
        }
        
        private static async Task SearchAndDownloadBooks(string authorName)
        {
            try{
                string searchUrl = $"{gutenbergBaseUrl}/ebooks/search/?querry={HttpUtility.HtmlEncode(authorName)}";
                string html = await client.GetStringAsync(searchUrl);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                
                HtmlNodeCollection resultNodes = document.DocumentNode.SelectNodes("//li[@class='booklink']//a[@class='link']");

                if(resultNodes != null || resultNodes.Count == 0)
                {
                    Console.WriteLine("Ничего не найдено.");
                    foreach (HtmlNode resultNode in resultNodes)
                    {
                        string title = HtmlEntity.DeEntitize(resultNode.SelectSingleNode(".//span[@class='title']").InnerText.Trim());
                        string relativeUrl = resultNode.GetAttributeValue("href", "");
                        string absoluteUrl = $"{gutenbergBaseUrl}{relativeUrl}";
                        Console.WriteLine($"Название книги: {title}, Абсолютная ссылка: {absoluteUrl}");
                    }
                    Console.WriteLine("Y/n");
                    string userInput = Console.ReadLine();

                    if (userInput.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (HtmlNode resultNode in resultNodes)
                        {
                            string title = HtmlEntity.DeEntitize(resultNode.SelectSingleNode(".//span[@class='title']").InnerText.Trim());
                            string relativeUrl = resultNode.GetAttributeValue("href", "");
                            string bookId = relativeUrl.Split('/')[2];
                            string downloadUrl = $"{gutenbergBaseUrl}/cache/epub/{bookId}/pg{bookId}.txt";
                            Console.WriteLine($"Название книги: {title}, Абсолютная ссылка: {downloadUrl}");
                            try
                            {
                                byte[] bookContent = await client.GetByteArrayAsync(downloadUrl);
                                string fileName = $"{title}.txt";
                                await File.WriteAllBytesAsync(fileName, bookContent);
                                Console.WriteLine($"книга {title}, в файл {fileName}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка скачивания книги: {ex.Message}");
                            }
                        }
                    }
                    else{
                        Console.WriteLine("Прервано пользователем.");
                    }  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске: {ex.Message}");
            }
        }
    }
}
