using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScrumPoker.API.Settings;
using ScrumPoker.Application.Features.Commands.CreateRoom;
using ScrumPoker.Application.Features.Queries.GetGameState;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.Controllers;

[ApiController]
[Route("api/warmup")]
public sealed class WarmupController : ControllerBase
{
    [HttpHead]
    public async Task<IActionResult> Warmup(
        [FromServices] IMessageBus bus,
        [FromServices] IOptions<WarmupSettings> settings)
    {
        var ttl = TimeSpan.FromSeconds(settings.Value.ExpirationSeconds);
        var room = await bus.InvokeAsync<Room>(
            new CreateRoomCommand("warmup", ["1", "2"], ttl, IsWarmup: true));
        _ = await bus.InvokeAsync<GetGameStateResult>(new GetGameStateQuery(room.Id));
        return Ok(room.Id);
    }
}
