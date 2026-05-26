using Microsoft.AspNetCore.Mvc;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.Vote;
using ScrumPoker.Application.Features.CreateRoom;
using ScrumPoker.Application.Features.JoinRoom;
using ScrumPoker.Application.Features.Vote;
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
    [ProducesResponseType(typeof(GameStateResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var command = new CreateRoomCommand(request.PlayerName);
        var room = await _bus.InvokeAsync<Domain.Rooms.Room>(command, ct);
        return Created($"/rooms/{room.Id}", GameStateMapper.ToResponse(room));
    }

    [HttpPost("{id:int}/join")]
    [ProducesResponseType(typeof(GameStateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
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
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/vote")]
    [ProducesResponseType(typeof(GameStateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Vote([FromRoute] int id, [FromBody] VoteRequest request, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var command = new VoteCommand(id, request.PlayerName, request.Value);
        var result = await _bus.InvokeAsync<VoteResult>(command, ct);

        return result switch
        {
            VoteResult.Success(var room) => Ok(GameStateMapper.ToResponse(room)),
            VoteResult.RoomNotFound => NotFound(),
            VoteResult.PlayerNotInRoom => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }
}
