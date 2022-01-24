namespace WebApplication
{
    public class ServicesLocation   
    {
        public string OtelCollector { get; init; }
        public string OtelCollectorHealthCheck { get; init; }
        
        public string ElasticsearchUriHealthCheck { get; init; }
        public string KibanaUriHealthCheck { get; init; }
        public string PrometheusUriHealthCheck { get; init; }
        public string GrafanaUriHealthCheck { get; init; }
        
        public string ZipkinUriHealthCheck { get; init; }
        public string JaegerUriHealthCheck { get; init; }

        public string WebApplication { get; init; }
    }
}