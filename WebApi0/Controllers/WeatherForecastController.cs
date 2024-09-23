using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using SharedConstants;

namespace WebApi0.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            using var activity = DiagnosticConfig.Source.StartActivity("Get-Weather-Forecast");
            Activity.Current?.SetTag(DiagnosticNames.ProductCategoryId, 100);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        public ActionResult GetWithSpanEvent()
        {
            Activity.Current?.AddEvent(new ActivityEvent("Validation field",
                tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>{
                    new("model.type", nameof(GetWithSpanEvent)),
                    new("model.field.field0", nameof(GetWithSpanEvent))
                    })
                ));
            return Ok();
        }

        public ActionResult GetWithSpanWithException()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {

                Activity.Current?.SetStatus(ActivityStatusCode.Error;
                Activity.Current?.RecordException(ex, new TagList
                {
                    { "product.id", 200}
                });
                return Ok();
            }
        }


        public ActionResult GetWithCreatingSpanLinks()
        {
            var currentContext = Activity.Current?.Context ?? new ActivityContext();
            Propagators
            Task.Run(() =>
            {
                using var activity = DiagnosticConfig.Source.StartActivity("SendEmail",
                                                                           ActivityKind.Internal,
                                                                           new ActivityContext(),
                                                                           links: new List<ActivityLink> { new ActivityLink(currentContext) });
            });
            return Ok();
        }

    }
}
