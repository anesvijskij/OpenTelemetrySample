namespace WebApplication
{
    public class OpenWeather
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public OpenWeatherForecast[] daily { get; set; }
    }
}