using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;



namespace TestVSCode
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("любую клавишу");
            Console.ReadKey();
            string hamletText = await DownloadHamletAsync();
            Console.Clear();
            Console.WriteLine("загружено");
            Console.WriteLine(hamletText);
        }
        static async Task<string> DownloadHamletAsync()
        {
            string habletUrl = "https://www.gutenberg.org/files/1524/1524-0.txt";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(habletUrl);
                    response.EnsureSuccessStatusCode();
                    string hamletText = await response.Content.ReadAsStringAsync();
                    return hamletText;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Ошибка при загрузке «Гамлета»: {e.Message}");
                    return "Не удалось загрузить «Гамлета». Проверьте подключение к Интернету.";
                }
            }
        }


    }
 }