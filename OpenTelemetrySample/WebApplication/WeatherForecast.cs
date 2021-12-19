using System;

namespace WebApplication
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }

    public class OpenWeather
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public OpenWeatherForecast[] daily { get; set; }
    }

    public class OpenWeatherForecast
    {
        public long dt { get; set; }
        public long sunrise { get; set; }
        public long sunset { get; set; }
        public long moonrise { get; set; }
        public long moonset { get; set; }
        public decimal moon_phase { get; set; }
        public OpenWeatherTemperatureStat temp { get; set; }
        public OpenWeatherTemperature feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public decimal dew_point { get; set; }
        public decimal wind_speed { get; set; }
        public decimal wind_deg { get; set; }
        public decimal wind_gust { get; set; }

        public decimal clouds { get; set; }
        public decimal pop { get; set; }
        public decimal rain { get; set; }
        public decimal uvi { get; set; }
        public OpenWeatherWeather[] weather { get; set; }
    }

    public class OpenWeatherTemperature
    {
        public decimal day { get; set; }
        public decimal night { get; set; }
        public decimal eve { get; set; }
        public decimal morn { get; set; }
    }

    public class OpenWeatherTemperatureStat : OpenWeatherTemperature
    {
        public decimal min { get; set; }
        public decimal max { get; set; }
    }

    public class OpenWeatherWeather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
}