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
            Console.WriteLine("введите текст для поиска (название книги)");
            string searchText = Console.ReadLine();

            await SearchGutenberg(searchText);
        }

        static async Task SearchGutenberg(string searchText)
        {
            try {
                string searchUrl = $"{gutenbergBaseUrl}/ebooks/search/?query={HttpUtility.HtmlEncode(searchText)}";

                string html = await client.GetStringAsync(searchUrl);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);

                HtmlNodeCollection resultNodes = document.DocumentNode.SelectNodes("//li[@class='booklink']/a[@class='link']");

                if (resultNodes!= null && resultNodes.Count > 0)
                {
                    Console.WriteLine("результаты");
                    foreach (HtmlNode resultNode in resultNodes)
                    {
                        string title = HtmlEntity.DeEntitize(resultNode.SelectSingleNode(".//span[@class='title']").InnerHtml.Trim());
                        string relativeUrl = resultNode.GetAttributeValue("href", "");
                        string absoluteUrl = $"{gutenbergBaseUrl}{relativeUrl}";
                        Console.WriteLine($"название: {title}, url {absoluteUrl}");
                    }
                }
                else
                {
                    Console.WriteLine("не найдено");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ошибка загрузки страницы: {ex.Message}");
            }

            }
    }
}
