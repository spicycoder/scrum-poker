using Microsoft.AspNetCore.Mvc;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.LeaveRoom;
using ScrumPoker.API.Features.Vote;
using ScrumPoker.Application.Features.Commands.CreateRoom;
using ScrumPoker.Application.Features.Commands.JoinRoom;
using ScrumPoker.Application.Features.Commands.LeaveRoom;
using ScrumPoker.Application.Features.Commands.RevealVotes;
using ScrumPoker.Application.Features.Commands.ResetVotes;
using ScrumPoker.Application.Features.Commands.Vote;
using ScrumPoker.Application.Features.Queries.GetGameState;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RoomController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var command = new CreateRoomCommand(request.PlayerName, request.CardSet);
        var room = await _bus.InvokeAsync<Room>(command, ct);
        Response.Headers.Location = $"/api/rooms/{room.Id}";
        return StatusCode(201);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GameStateResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken ct)
    {
        var room = await _bus.InvokeAsync<Room?>(new GetGameStateQuery(id), ct);
        return room is null ? NotFound() : Ok(GameStateMapper.ToResponse(room));
    }

    [HttpPost("{id:int}/join")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Join([FromRoute] int id, [FromBody] JoinRoomRequest request, CancellationToken ct)
    {
        try
        {
            var room = await _bus.InvokeAsync<Room?>(new JoinRoomCommand(id, request.PlayerName), ct);
            return room is null ? NotFound() : StatusCode(201);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already in room"))
        {
            return Conflict();
        }
    }

    [HttpPost("{id:int}/vote")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Vote([FromRoute] int id, [FromBody] VoteRequest request, CancellationToken ct)
    {
        try
        {
            var room = await _bus.InvokeAsync<Room?>(new VoteCommand(id, request.PlayerName, request.Value), ct);
            return room is null ? NotFound() : StatusCode(201);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/reveal")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reveal([FromRoute] int id, CancellationToken ct)
    {
        var room = await _bus.InvokeAsync<Room?>(new RevealVotesCommand(id), ct);
        return room is null ? NotFound() : StatusCode(201);
    }

    [HttpPost("{id:int}/reset")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reset([FromRoute] int id, CancellationToken ct)
    {
        var room = await _bus.InvokeAsync<Room?>(new ResetVotesCommand(id), ct);
        return room is null ? NotFound() : StatusCode(201);
    }

    [HttpPost("{id:int}/leave")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Leave([FromRoute] int id, [FromBody] LeaveRoomRequest request, CancellationToken ct)
    {
        try
        {
            var room = await _bus.InvokeAsync<Room?>(new LeaveRoomCommand(id, request.PlayerName), ct);
            return room is null ? NotFound() : NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
