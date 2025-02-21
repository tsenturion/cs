using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestVSCode
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private const string apiKey = "";        
        static async Task Main(string[] args)
        {
            Console.WriteLine("1 - человек / 2 - бот");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                await PlayerVsPlayer();
            }
            else if (choice == "2") {
                await BotVsPlayer();
            }
            else{
                Console.WriteLine("Неправильный ввод. Попробуйте еще раз.");
            }
            Console.ReadKey();
        }
        
        private static async Task PlayerVsPlayer()
        {
            Console.WriteLine("человек человек");
            Console.WriteLine("для броска человек 1 enter");
            Console.ReadLine();
            int player1roll = await RollDice();
            Console.WriteLine($"Кубик выпал: {player1roll}");
            Console.WriteLine("для броска человек 2 enter");
            Console.ReadLine();
            int player2roll = await RollDice();
            Console.WriteLine($"Кубик выпал: {player2roll}");
            DetermineWinner(player1roll, player2roll);
        }
        
        private static async Task<int> RollDice()
        {
            JObject requestParams = new JObject
            {
                ["apiKey"] = apiKey,
                ["n"] = 1,
                ["min"] = 1,
                ["max"] = 6,
                ["replacement"] = true
            };
            JObject requestBody = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["method"] = "generateIntegers",
                ["params"] = requestParams,
                ["id"] = 1
            };

            string jsonBody = requestBody.ToString(Formatting.None);

            var response = await client.PostAsync(
                "https://api.random.org/json-rpc/4/invoke",
                new StringContent(jsonBody, Encoding.UTF8, "application/json")
            );

            var resultJson = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(resultJson);
            return result.result.random.data[0];
        }
        
        private static void DetermineWinner(int roll1, int roll2)
        {
            if (roll1 > roll2)
            {
                Console.WriteLine("первый выиграл");
            }
            else if (roll1 < roll2)
            {
                Console.WriteLine("второй выиграл");
            }
            else
            {
                Console.WriteLine("ничья");
            }
        }
        
        private static async Task BotVsPlayer()
        {
            Console.WriteLine("человек бот");
            Console.WriteLine("для броска человека 1 enter");
            Console.ReadLine();
            int player1roll = await RollDice();
            Console.WriteLine($"Кубик выпал: {player1roll}");
            Random random = new Random();
            int botRoll = random.Next(1, 7);
            Console.WriteLine($"Кубик выпал бота: {botRoll}");
            DetermineWinner(player1roll, botRoll);
        }
    }
}
