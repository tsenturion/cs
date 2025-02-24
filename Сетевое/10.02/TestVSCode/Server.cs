using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace TestVSCode
{
    class Program
    {     
        static async Task Main(string[] args)
        {
            string url = "https://www.gutenberg.org/";
            string searchText = "book";
            int maxTimeSeconds = 10;

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxTimeSeconds));
            var task = GetHtmlAsync(url, cts.Token);
            Console.WriteLine("p - pause, r - resume, c - cansel");
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.KeyChar.ToString().ToLower())
                {
                    case "p":
                        cts.Cancel();
                        Console.WriteLine("Paused.");
                        break;
                    case "r":
                        cts.Dispose();
                        cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxTimeSeconds));
                        task = GetHtmlAsync(url, cts.Token);
                        Console.WriteLine("Resumed.");
                        break;
                    case "c":
                        cts.Cancel();
                        Console.WriteLine("Cancelling.");
                        return;
                    default:
                        break;
                }
                if (task.IsCompleted)
                {
                    break;
                }
            }

            if (task.IsCanceled)
            {
                Console.WriteLine("Task cancelled.");
            }
            else if (task.IsFaulted)
            {
                Console.WriteLine($"Task failed: {task.Exception.Flatten().Message}");
            }
            else
            {
                var html = await task;
                if (html != null)
                {
                    if (html.Contains(searchText))
                    {
                        Console.WriteLine("Text found on the page.");
                        int index = html.IndexOf(searchText);
                        int fragmentStart = Math.Max(0, index - 50);
                        int fragmentEnd = Math.Min(100, html.Length - fragmentStart);
                        string fragment = html.Substring(fragmentStart, fragmentEnd);
                        Console.WriteLine($"Fragment: {fragment}");
                    }
                    else
                    {
                        Console.WriteLine("Text not found on the page.");
                    }
                    // reader.Close();
                    // dataStream.Close();
                    // response.Close();
                }
                else{
                    Console.WriteLine("Failed to get HTML content за 10 секунд.");
                }
                // WebRequest request = WebRequest.Create(url);
                // HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Stream dataStream = response.GetResponseStream();
                // StreamReader reader = new StreamReader(dataStream);
                //string html = reader.ReadToEnd();
            }
        }
        
        static async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken)
        {
            try{
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url, cancellationToken);
                if(response.IsSuccessStatusCode){
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }
                else{
                    Console.WriteLine($"Failed to get HTML content: {response.StatusCode}");
                    return null;
                }
            }
            catch(OperationCanceledException ex){
                Console.WriteLine($"Task cancelled: {ex.Message}");
                return null;
            }
            catch(Exception ex){
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
