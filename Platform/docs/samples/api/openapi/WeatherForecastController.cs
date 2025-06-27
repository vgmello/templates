using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Get the current weather forecast.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a 5-day weather forecast.
        /// </remarks>
        /// <response code="200">Returns the weather forecast.</response>
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<WeatherForecast>> Get()
        {
            return Ok(new WeatherForecast());
        }
    }
}