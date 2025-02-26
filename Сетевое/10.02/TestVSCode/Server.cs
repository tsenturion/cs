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
            string apiKey = "";
            string searchId = "";
            string apiUrl = $"https://customsearch.googleapis.com/customsearch/v1?cx={searchId}&key={apiKey}&q={query}&start=0";
            try{
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
