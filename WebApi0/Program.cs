
using IziHardGames.Observing.Tracing;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using IziMetrics;
using WebApi0.BackgroundServices;

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
            //builder.Services.AddDbContextPool<SimpleDbContext>(x => x.UseInMemoryDatabase("iziinmemory"));
            var evUser = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var evPwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var evServer = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var evPort = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            builder.Services.AddDbContextPool<SimpleDbContext>(x => x.UseNpgsql($"server={evServer};uid={evUser};pwd={evPwd};database=IziTest"));

            builder.Services.AddHostedService<SomeGarbageProdcuer>();
            builder.Services.AddOpenTelemetry()
                .AddGraphanaAndPrometheusToOpenTelemetry()
                .AddZipkin(new OtlpParams()
            {
                HostName = "localhost",
                ServiceName = "IziTestOtlpTracingsService",
                MainSourceName = "IziTestOtlpTracingsSource",
                SubSourcesNames = new string[] { ActivityExample.SourceName }
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
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.Run();
        }
    }
}
