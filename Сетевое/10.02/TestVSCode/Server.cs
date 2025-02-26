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
         Создайте приложение для отображения информации о 
        фильме. Пользователь вводит название фильма, приложение 
        отображает информацию о нём. Для поиска информации 
        о фильме используйте http://www.omdbapi.com/.
        */
        private const string ApiKey = "f360ac11";

        private const string ApiUrl = "http://www.omdbapi.com/";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter movie title:");
            string movieTitle = Console.ReadLine();
            if (!string.IsNullOrEmpty(movieTitle))
            {
                await GetMovieInfo(movieTitle);
            }
            else{
                Console.WriteLine("Invalid title!");
            }
        }
        
        private static async Task GetMovieInfo(string title)
        {
            try{
                using (HttpClient client = new HttpClient())
                {
                    string requstUrl = $"{ApiUrl}?t={title}&apikey={ApiKey}";
                    HttpResponseMessage response = await client.GetAsync(requstUrl);
                    response.EnsureSuccessStatusCode();
                    string jsonString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(jsonString);
                    if(json["Response"].Value<string>() == "True")
                    {
                        Console.WriteLine($"Title: {json["Title"]}");
                        Console.WriteLine($"Year: {json["Year"]}");
                        Console.WriteLine($"Rated: {json["Rated"]}");
                        Console.WriteLine($"Genre: {string.Join(", ", json["Genre"])}");
                        Console.WriteLine($"Director: {json["Director"]}");
                        Console.WriteLine($"Actors: {json["Actors"]}");
                        Console.WriteLine($"Plot: {json["Plot"]}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {json["Error"]}");
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