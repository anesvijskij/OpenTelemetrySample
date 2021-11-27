using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace WebApplication
{
    public class Startup
    {
        public static readonly ActivitySource WebApplicationActivitySource = new ActivitySource("WebApplicationSource");
        public static readonly Meter WebApplicationMeter = new Meter("WebApplicationMeter");
        public static readonly Counter<int> RequestCounter;
        public static readonly Histogram<float> RequestDurationHistogram;

        static Startup()
        {
            RequestCounter = WebApplicationMeter.CreateCounter<int>("Requests");
            RequestDurationHistogram = WebApplicationMeter.CreateHistogram<float>("RequestDuration", unit: "ms");
            WebApplicationMeter.CreateObservableGauge("ThreadCount",
                () => new[] { new Measurement<int>(ThreadPool.ThreadCount) });
        }
        public Startup(IConfiguration configuration)
        {
            
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // metric WebApplication
            services.AddOpenTelemetryMetrics(builder =>
            {
                builder.AddHttpClientInstrumentation();
                builder.AddAspNetCoreInstrumentation();
                
                builder.AddMeter("WebApplicationMeter");
                builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
            });
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.AddHttpClientInstrumentation();
                builder.AddAspNetCoreInstrumentation();
                
                builder.AddSource("WebApplicationSource");
                builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
            });
            services.AddHttpClient();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}