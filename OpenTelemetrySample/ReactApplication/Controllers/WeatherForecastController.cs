using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApplication;

namespace ReactApplication.Controllers
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
        private readonly string _webApplicationServiceLocation;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory,IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            
            var servicesLocation = configuration.GetSection("Services").Get<ServicesLocation>();
            _webApplicationServiceLocation = servicesLocation?.WebApplication ?? "localhost:49166";
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _httpClientFactory.CreateClient()
                .GetFromJsonAsync<IEnumerable<WeatherForecast>>(_webApplicationServiceLocation + "/WeatherForecast");
        }
    }
}