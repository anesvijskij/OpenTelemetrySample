using System.Diagnostics.Metrics;
using System.Threading;

namespace WebApplication.Telemetry
{
    /// <summary>
    /// All Custom meters, counters, etc
    /// </summary>
    public static class MeterData
    {
        public static readonly Meter WebApplicationMeter = new("WebApplicationMeter");
        public static readonly Counter<int> RequestCounter = WebApplicationMeter.CreateCounter<int>("Requests");
        public static readonly Histogram<float> RequestDurationHistogram = WebApplicationMeter.CreateHistogram<float>("RequestDuration", unit: "ms");
        
        static  MeterData()
        {
            WebApplicationMeter.CreateObservableGauge("ThreadCount",
                () => new[] { new Measurement<int>(ThreadPool.ThreadCount) });
        }
    }
}