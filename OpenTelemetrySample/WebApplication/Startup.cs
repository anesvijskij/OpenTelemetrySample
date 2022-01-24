using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers();
            
            services
                // Using an absolute URI with localhost because of https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/410
                .AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint(HealthCheckTags.Application, "http://localhost/health");
                    setup.AddHealthCheckEndpoint(HealthCheckTags.Infrastructure, "http://localhost/health-infrastructure");
                })
                .AddInMemoryStorage();
            
            var servicesLocation = Configuration.GetSection("Services").Get<ServicesLocation>();

            services.AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(1024, "Allocated Memory",
                    tags: new[] { HealthCheckTags.Application, HealthCheckTags.Memory })

                .AddElasticsearch(servicesLocation.ElasticsearchUriHealthCheck, "Elasticsearch",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.ELK })
                .AddUrlGroup(new Uri(servicesLocation.KibanaUriHealthCheck), "Kibana",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.ELK })
                .AddUrlGroup(new Uri(servicesLocation.PrometheusUriHealthCheck), "Prometheus",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.Monitoring })
                .AddUrlGroup(new Uri(servicesLocation.GrafanaUriHealthCheck), "Grafana",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.Monitoring })
                .AddUrlGroup(new Uri(servicesLocation.OtelCollectorHealthCheck), "OTel Collector",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.Monitoring })
                .AddUrlGroup(new Uri(servicesLocation.ZipkinUriHealthCheck), "Zipkin",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.Monitoring })
                .AddUrlGroup(new Uri(servicesLocation.JaegerUriHealthCheck), "Jaeger",
                    tags: new[] { HealthCheckTags.Infrastructure, HealthCheckTags.Monitoring });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication", Version = "v1" });
            });
            

            
            string serviceName = Environment.GetEnvironmentVariable("WEB_APPLICATION_SERVICE_NAME") ?? typeof(Startup).Assembly.GetName().FullName;
            
            // Support collector in insecure way
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            // metric WebApplication
            services.AddOpenTelemetryMetrics(builder =>
            {
                builder.AddHttpClientInstrumentation();
                builder.AddAspNetCoreInstrumentation();
                
                builder.AddMeter("WebApplicationMeter");

                if (!string.IsNullOrEmpty(servicesLocation.OtelCollector))
                    builder.AddOtlpExporter(options => options.Endpoint = new Uri(servicesLocation.OtelCollector));
                else
                {
                    builder.AddConsoleExporter();
                }
            });
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.AddHttpClientInstrumentation();
                builder.AddAspNetCoreInstrumentation();

                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                    serviceName,
                    serviceVersion: typeof(Startup).Assembly.GetName().Version?.ToString()));

                if (!string.IsNullOrEmpty(servicesLocation.OtelCollector))
                    builder.AddOtlpExporter(options => options.Endpoint = new Uri(servicesLocation.OtelCollector));
                else
                {
                    builder.AddConsoleExporter();
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // TODO: do we need it?
            app.UseSerilogRequestLogging();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            
            app.UseSwagger();
            app.UseSwaggerUI(c => 
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication v1"));

            app.UseRouting();
            
            // TODO: do we need it?
            app.UseHttpMetrics();

            app.UseAuthorization();
            
            // TODO: do we need it?
            // Choose AspNetCore.HealthChecks.Prometheus.Metrics over prometheus-net.AspNetCore.HealthChecks
            // it exports not only the status but also the health check duration.
            app.UseHealthChecksPrometheusExporter("/metrics");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Application)
                });
                endpoints.MapHealthChecks("/health-infrastructure", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Infrastructure)
                });
                
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/health-ui";
                });
                
                // TODO: do we need it?
                endpoints.MapMetrics(); // Prometheus
            });
            
        }
    }
}