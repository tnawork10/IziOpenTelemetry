
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebApi0
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddHttpClient("ZipkinExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
            var atrs = new Dictionary<string, object>() {
                        {"service.name", DiagnosticConfig.ServiceName},
                        {"host.name",DiagnosticConfig.HostName}
                    };

            builder.Services.AddOpenTelemetry()
                .WithTracing(builder =>
                {

                    builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(atrs))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(DiagnosticConfig.SourceName)
                    .AddConsoleExporter()
                    .AddZipkinExporter(f => f.HttpClientFactory = () =>
                    {
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value");
                        return client;
                    });
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
