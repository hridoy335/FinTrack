using FinTrackCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Api.Controllers;

[Route("api/[controller]s")]
public class HealthController(AppDbContext dbContext) : JsonApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return SendResponse(
            StatusCodes.Status200OK,
            string.Empty,
            new
            {
                status = canConnect ? "Healthy" : "Degraded",
                architecture = "Clean Architecture",
                database = canConnect ? "Connected" : "Unavailable"
            });
    }
}
