using FinTrackCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]s")]
public class HealthController : JsonApiControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(ct);

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
