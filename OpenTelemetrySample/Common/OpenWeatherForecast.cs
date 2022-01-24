namespace WebApplication
{
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
}