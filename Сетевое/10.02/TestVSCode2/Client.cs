using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class TestVSCode2
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiKey = "93907408-f04d-4491-9442-450c3f0a62f7"; // Замените на ваш API ключ

    static async Task Main(string[] args)
    {
        Console.WriteLine("Выберите режим игры: 1 - Человек против человека, 2 - Человек против компьютера");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            await PlayerVsPlayer();
        }
        else if (choice == "2")
        {
            await PlayerVsComputer();
        }
        else
        {
            Console.WriteLine("Неверный выбор.");
        }
    }

    private static async Task PlayerVsPlayer()
    {
        Console.WriteLine("Игрок 1, нажмите Enter, чтобы бросить кости.");
        Console.ReadLine();
        int player1Roll = await RollDice();
        Console.WriteLine($"Игрок 1 бросил: {player1Roll}");

        Console.WriteLine("Игрок 2, нажмите Enter, чтобы бросить кости.");
        Console.ReadLine();
        int player2Roll = await RollDice();
        Console.WriteLine($"Игрок 2 бросил: {player2Roll}");

        DetermineWinner(player1Roll, player2Roll);
    }

    private static async Task PlayerVsComputer()
    {
        Console.WriteLine("Нажмите Enter, чтобы бросить кости.");
        Console.ReadLine();
        int playerRoll = await RollDice();
        Console.WriteLine($"Вы бросили: {playerRoll}");

        Random rnd = new Random();
        int computerRoll = rnd.Next(1, 7); // Симуляция броска компьютера
        Console.WriteLine($"Компьютер бросил: {computerRoll}");

        DetermineWinner(playerRoll, computerRoll);
    }

private static async Task<int> RollDice()
{
    // Создаем объект JObject для параметров запроса
    JObject requestParams = new JObject
    {
        ["apiKey"] = apiKey,
        ["n"] = 1,
        ["min"] = 1,
        ["max"] = 6,
        ["replacement"] = true
    };

    // Создаем основной объект запроса
    JObject requestBody = new JObject
    {
        ["jsonrpc"] = "2.0",
        ["method"] = "generateIntegers",
        ["params"] = requestParams,
        ["id"] = 42
    };

    // Преобразуем объект в JSON строку
    string jsonBody = requestBody.ToString(Newtonsoft.Json.Formatting.None);

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
            Console.WriteLine("Первый игрок выиграл!");
        else if (roll2 > roll1)
            Console.WriteLine("Второй игрок выиграл!");
        else
            Console.WriteLine("Ничья!");
    }
}
