using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace IziHardGames.Observing.Tracing
{
    public static class ExtensionsForIServiceCollectionAsOpenTelemetry
    {
        public static void AddZipkin(this IServiceCollection services, OtlpParams otlpParams)
        {
            var ev = Environment.GetEnvironmentVariable("GCE_OTLP_ENABLE_ZIPKIN");
            if (ev is null || !ev.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            var atrs = new Dictionary<string, object>() {
                        {"service.name",otlpParams.ServiceName },
                        {"host.name", otlpParams.HostName},
                    };

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {

                    for (int i = 0; i < otlpParams.SubSourcesNames.Length; i++)
                    {
                        builder.AddSource(otlpParams.SubSourcesNames[i]);
                    }

                    builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(atrs))
                    .AddEntityFrameworkCoreInstrumentation(opt =>
                    {
                        opt.EnrichWithIDbCommand = (activity, command) =>
                        {
                            //activity.DisplayName = command.CommandText;
                            activity.AddTag(nameof(command.CommandText), command.CommandText);
                        };
                    })
                    //.AddCassandraInstrumentation() // not released yet
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            // Extract trace information
                            var traceId = activity.TraceId;
                            var spanId = activity.SpanId;

                            // Add trace information to the Correlation list
                            //CorrelationData.Correlation.Add(new KeyValuePair<string, string>("Activity.TraceId", traceId.ToHexString()));
                            //CorrelationData.Correlation.Add(new KeyValuePair<string, string>("Activity.SpanId", spanId.ToHexString()));

                            return;
                        };
                        o.EnrichWithHttpResponse = (activity, response) =>
                        {
                            //if (response.HttpContext.Request.Headers.TryGetValue(Header.ClientActionName, out StringValues clientActionHeaders))
                            //{
                            //}
                            //var displayName = $"{serviceRole}.Incoming.{clientActionHeaders}";
                            activity.DisplayName = response.HttpContext.Request.Host + response.HttpContext.Request.PathBase + response.HttpContext.Request.Path;
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource(otlpParams.MainSourceName)
                    .AddConsoleExporter()
                    .AddZipkinExporter(f => f.HttpClientFactory = () =>
                    {
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value");
                        return client;
                    });
                });
        }
    }

    public class OtlpParams
    {
        public string ServiceName { get; set; } = null!;
        public string HostName { get; set; } = "gce.ru";
        public string MainSourceName { get; set; } = null!;
        public string[] SubSourcesNames { get; set; } = Array.Empty<string>();
    }
}
