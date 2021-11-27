using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
           
            try
            {
                using var activity = Startup.WebApplicationActivitySource.StartActivity("WeatherForecast");
            
                Startup.RequestCounter.Add(1, new KeyValuePair<string, object?>("name", nameof(Get)));
            
                activity?.AddTag("Name", nameof(Get));
                activity?.AddBaggage("SampleContext", Guid.NewGuid().ToString());
                // outer request
                var stopwatch = Stopwatch.StartNew();
                await _httpClientFactory.CreateClient().GetStringAsync("https://ya.ru");
                Startup.RequestDurationHistogram.Record(stopwatch.ElapsedMilliseconds,
                    tag: new KeyValuePair<string, object?>("Host", "www.ya.ru"));
            
                var rng = new Random();
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = Summaries[rng.Next(Summaries.Length)]
                    })
                    .ToArray();
            }
            finally
            {
                
            }
            
        }
    }
}