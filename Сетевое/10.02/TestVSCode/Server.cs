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
            string city = "Москва";
            string apiKey = "3fded8a47f3c2bbddc3af0cce9a3120b";
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            try{
                var weatherData = await GetWeatherData(url);
                double lat = weatherData.coord.lat;
                double lon = weatherData.coord.lon;
                //string oneCallUrl = $"http://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lon}&exclude=hourly,minutely&appid={apiKey}&units=metric";
                string oneCallUrl = $"http://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lon}&appid={apiKey}&units=metric";
                var currentWeather = await GetCurrentWeather(oneCallUrl);

                Console.WriteLine($"City: {weatherData.name}, Temperature: {currentWeather.current.temp}°C, Humidity: {currentWeather.current.humidity}% описание {currentWeather.current.weather[0].description}");
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

        static async Task<WeaterResponse> GetWeatherData(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeaterResponse>(data);
        }
        
        static async Task<OneCallResponse> GetCurrentWeather(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OneCallResponse>(data);
        }
    }
    
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
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

    public class WeaterResponse
    {
        public int id { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public string name { get; set; }
        public Coord coord { get; set; }
        public int cod { get; set; }

        //public int country { get; set; }
        //public int population { get; set; }
    }
    
    public class OneCallResponse
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public Current current { get; set; }
        //public int dt { get; set; }
        //public List<Weather> current { get; set; }
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
