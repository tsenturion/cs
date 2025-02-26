using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

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
            string searchTerm = Console.ReadLine();
            string url = $"https://www.google.com/search?q={Uri.EscapeDataString(searchTerm)}";
            try{
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);
                    var results = htmlDoc.DocumentNode.SelectNodes("//div[@class='g']");
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            var titleNode = result.SelectSingleNode(".//h3");
                            var linkNode = result.SelectSingleNode(".//a");
                            var snippetNode = result.SelectSingleNode(".//span[@class='aCOpRe']");
                            if (titleNode!= null && linkNode!= null && snippetNode!= null)
                            {
                                string title = titleNode.InnerText;
                                string link = linkNode.GetAttributeValue("href", "");
                                string snippet = snippetNode.InnerText;
                                Console.WriteLine($"Title: {title}");
                                Console.WriteLine($"Link: {link}");
                                Console.WriteLine($"Snippet: {snippet}");
                                Console.WriteLine("--------------------------------");
                            }
                        }
                    }
                    else{
                        Console.WriteLine("No results found.");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error connecting to the API: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
