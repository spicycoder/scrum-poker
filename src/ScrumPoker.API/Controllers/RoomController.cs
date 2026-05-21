using Microsoft.AspNetCore.Mvc;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.Application.Features.CreateRoom;
using ScrumPoker.Application.Features.JoinRoom;
using Wolverine;

namespace ScrumPoker.API.Controllers;

[ApiController]
[Route("rooms")]
public sealed class RoomController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RoomController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var command = new CreateRoomCommand(request.PlayerName);
        var room = await _bus.InvokeAsync<Domain.Rooms.Room>(command, ct);
        return Created($"/rooms/{room.Id}", GameStateMapper.ToResponse(room));
    }

    [HttpPost("{id:int}/join")]
    public async Task<IActionResult> Join([FromRoute] int id, [FromBody] JoinRoomRequest request, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var command = new JoinRoomCommand(id, request.PlayerName);
        var result = await _bus.InvokeAsync<JoinRoomResult>(command, ct);

        return result switch
        {
            JoinRoomResult.Success(var room) => Ok(GameStateMapper.ToResponse(room)),
            JoinRoomResult.RoomNotFound => NotFound(),
            JoinRoomResult.PlayerAlreadyInRoom => Conflict(),
            _ => StatusCode(500)
        };
    }
}
