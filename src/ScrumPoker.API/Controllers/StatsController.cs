using Microsoft.AspNetCore.Mvc;
using ScrumPoker.Application.Abstractions;

namespace ScrumPoker.API.Controllers;

[ApiController]
[Route("api/stats")]
public sealed class StatsController(IStatsRepository stats) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await stats.GetStatsAsync(ct);
        return Ok(result);
    }
}
