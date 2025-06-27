using Microsoft.AspNetCore.Mvc;

namespace Operations.Samples.Api;

[ApiController]
[Route("[controller]")]
public class OpenApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello from OpenAPI");
    }
}
