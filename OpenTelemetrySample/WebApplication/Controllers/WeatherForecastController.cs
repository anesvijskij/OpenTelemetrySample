using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication.Telemetry;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(string latitude = "33.44", string longitude = "-94.04")
        {
            WeatherForecast[] result = null;
            try
            {
                using var scope = _logger.BeginScope("{Id}", Guid.NewGuid().ToString());

                var activity = Activity.Current;
            
                MeterData.RequestCounter.Add(1, new KeyValuePair<string, object?>("name", nameof(Get)));
                var appId = _configuration.GetSection("WeatherKeys").Get<WeatherKeys>()?.AppId; 
                
                activity?.AddTag("Name", nameof(Get));
                activity?.AddBaggage("SampleContext", Guid.NewGuid().ToString());
                // outer request
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var weatherResult = await _httpClientFactory.CreateClient()
                        .GetFromJsonAsync<OpenWeather>(
                            $"https://api.openweathermap.org/data/2.5/onecall?lat={latitude}&lon={longitude}&exclude=current,minutely,hourly,alerts&units=metric&lang=ru&appid={appId}");
                    
                    result = weatherResult?.daily.Select(it => new WeatherForecast()
                    {
                        Date = DateTime.FromBinary(it.dt), Summary = it.weather.FirstOrDefault()?.main,
                        TemperatureC = (int)it.temp.day
                    }).ToArray();
                }
                catch (Exception e)
                {
                    _logger.LogError("OpenWeatherMapError", e);
                    await _httpClientFactory.CreateClient().GetStringAsync("https://ya.ru");
                }
                finally
                {
                    MeterData.RequestDurationHistogram.Record(stopwatch.ElapsedMilliseconds,
                        tag: new KeyValuePair<string, object?>("Host", "www.ya.ru"));
                }

                activity?.AddEvent(new ActivityEvent("New Event",
                    tags: new ActivityTagsCollection(new KeyValuePair<string, object?>[]
                    {
                        new("Request finished", "api.openweathermap.org"), 
                        new(nameof(latitude), latitude),
                        new(nameof(longitude), longitude)
                    })));

                if (result == null)
                {
                    var rng = new Random();
                    result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                        {
                            Date = DateTime.Now.AddDays(index),
                            TemperatureC = rng.Next(-20, 55),
                            Summary = Summaries[rng.Next(Summaries.Length)]
                        })
                        .ToArray();
                }
            }
            finally
            {
                _logger.LogInformation("WeatherForecasts generated {Forecasts}", (object)result);
            }

            return result;
        }
    }

    public class WeatherKeys
    {
        public string AppId { get; init; } 
    }
}