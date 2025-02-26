using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestVSCode2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string city = "Moscow";
            string country = "ru";
            string apiKey = "99a9a75da640a2098df3bc7c5a91a721";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city},{country}&APPID={apiKey}&units=metric";

            try
            {
                var weatherData = await GetWeatherData(url);
                double lat = weatherData.coord.lat;
                double lon = weatherData.coord.lon;

                string oneCallUrl = $"https://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lon}&appid={apiKey}&units=metric";
                var forecastData = await GetForecastData(oneCallUrl);

                Console.WriteLine("Прогноз погоды на 7 дней:");
                foreach (var daily in forecastData.daily)
                {
                    DateTime date = DateTimeOffset.FromUnixTimeSeconds(daily.dt).DateTime;
                    Console.WriteLine($"{date.ToShortDateString()}: Температура: {daily.temp.day}°C, Описание: {daily.weather[0].description}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка подключения к API: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static async Task<WeatherResponse> GetWeatherData(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Запрос завершился с ошибкой: {response.StatusCode}");
            }
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(data);
        }

        static async Task<OneCallResponse> GetForecastData(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Запрос завершился с ошибкой: {response.StatusCode}");
            }
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OneCallResponse>(data);
        }
    }

    public class Coord
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public double message { get; set; }
    }

    public class WeatherResponse
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Daily
    {
        public long dt { get; set; }
        public Temp temp { get; set; }
        public List<Weather> weather { get; set; }
    }

    public class Temp
    {
        public double day { get; set; }
    }

    public class OneCallResponse
    {
        public List<Daily> daily { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public Current current { get; set; }
    }

    public class Current
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public List<Weather> weather { get; set; }
    }
}
