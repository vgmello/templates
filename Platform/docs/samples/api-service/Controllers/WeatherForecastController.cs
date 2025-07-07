using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

/// <summary>
/// APIs for weather forecasts.
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Gets the current weather forecast.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a 5-day forecast.
    /// </remarks>
    /// <response code="200">Returns the weather forecast.</response>
    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();
        return Ok(forecast);
    }
}
