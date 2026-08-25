using Microsoft.AspNetCore.Mvc;

namespace FinTrackCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy", architecture = "Clean Architecture" });
}
