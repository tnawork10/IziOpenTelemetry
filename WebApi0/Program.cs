
using IziHardGames.Observing.Tracing;
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

            builder.Services.AddZipkin(new OtlpParams()
            {
                HostName = "localhost",
                ServiceName = "IziTestOtlpTracingsService",
                SourceName = "IziTestOtlpTracingsSource"
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
