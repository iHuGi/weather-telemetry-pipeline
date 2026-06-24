using Newtonsoft.Json.Linq;
using DotNetEnv;
using System.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherApp
{
    // ---------------------------------------------------------
    // 1. DATA MODEL (DTO)
    // ---------------------------------------------------------
    public class WeatherData
    {
        public string? CityName { get; set; }
        public double Temperature { get; set; }
        public string? Condition { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    // ---------------------------------------------------------
    // 2. SERVICE LAYER (API CLIENT)
    // ---------------------------------------------------------
    public class WeatherClient
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string _apiKey;

        public WeatherClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<WeatherData?> GetWeatherAsync(string city)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";

            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            // Fail fast: return null if API call failed
            if (!response.IsSuccessStatusCode)
                return null;

            string responseBody = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(responseBody);

            // Map JSON → DTO
            return new WeatherData
            {
                CityName = (string?)data["name"],
                Temperature = (double)data["main"]!["temp"]!,
                Condition = (string?)data["weather"]![0]!["main"],
                Latitude = (double)data["coord"]!["lat"]!,
                Longitude = (double)data["coord"]!["lon"]!
            };
        }
    }

    // ---------------------------------------------------------
    // 3. ENTRY POINT
    // ---------------------------------------------------------
    class Program
    {
        static async Task Main(string[] args)
        {
            // -----------------------------------------------------
            // 1. START TIMER AT THE ABSOLUTE BEGINNING
            // -----------------------------------------------------
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("--- OOP WEATHER PIPELINE BOOTING UP ---");

            // Load environment variables (API key hidden in .env file)
            Env.Load();
            string apiKey = Env.GetString("WEATHER_API");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.Error.WriteLine("[!] CRITICAL: WEATHER_API variable not found.");
                return;
            }

            // Input dataset
            List<string> cities = new() {
                "Aveiro","Beja","Braga","Braganca","Castelo Branco",
                "Coimbra","Evora","Faro","Funchal","Guarda",
                "Leiria","Lisbon","Ponta Delgada","Portalegre","Porto",
                "Santarem","Setubal","Viana do Castelo","Vila Real","Viseu"
             };

            // Service instantiation (dependency injection style)
            WeatherClient weatherTool = new(apiKey);

            // -----------------------------------------------------
            // FIRE ALL REQUESTS IN PARALLEL (async fan-out pattern)
            // -----------------------------------------------------
            List<Task<WeatherData?>> tasks = cities
                .Select(city => weatherTool.GetWeatherAsync(city))
                .ToList();

            // The massive internet wait happens here
            WeatherData?[] results = await Task.WhenAll(tasks);

            // -----------------------------------------------------
            // RESULTS PROCESSING (index-safe mapping)
            // -----------------------------------------------------
            for (int i = 0; i < results.Length; i++)
            {
                try
                {
                    var result = results[i];
                    var city = cities[i];

                    if (result is not null)
                    {
                        Console.WriteLine($">> {result.CityName} <<");
                        Console.WriteLine($" Temp : {result.Temperature} C");
                        Console.WriteLine($" Cond : {result.Condition}");
                        Console.WriteLine("-----------------------------------");
                    }
                    else
                    {
                        Console.Error.WriteLine($"[!] Failed to fetch data for {city}. Non-200 response.");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.Error.WriteLine($"[!] Network exception while processing {cities[i]}: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[!] Unexpected error processing {cities[i]}: {ex.Message}");
                }
            }

            // -----------------------------------------------------
            // 2. STOP TIMER AT THE ABSOLUTE END
            // -----------------------------------------------------
            timer.Stop();

            // 3. Output the total time taken
            Console.WriteLine($"\nTotal C# End-to-End time: {timer.ElapsedMilliseconds} milliseconds.");
        }
    }
}