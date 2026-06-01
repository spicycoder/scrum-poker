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
        var command = new CreateRoomCommand(request.PlayerName);
        var room = await _bus.InvokeAsync<Domain.Rooms.Room>(command, ct);
        Response.Headers.Location = $"/api/rooms/{room.Id}";
        return StatusCode(201);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GameStateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var result = await _bus.InvokeAsync<GetGameStateResult>(new GetGameStateQuery(id), ct);

        return result switch
        {
            GetGameStateResult.Success(var room) => Ok(GameStateMapper.ToResponse(room)),
            GetGameStateResult.RoomNotFound => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/join")]
    [ProducesResponseType(201)]
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
            JoinRoomResult.Success => StatusCode(201),
            JoinRoomResult.RoomNotFound => NotFound(),
            JoinRoomResult.PlayerAlreadyInRoom => Conflict(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/vote")]
    [ProducesResponseType(201)]
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
            VoteResult.Success => StatusCode(201),
            VoteResult.RoomNotFound => NotFound(),
            VoteResult.PlayerNotInRoom => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/reveal")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reveal([FromRoute] int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var result = await _bus.InvokeAsync<RevealVotesResult>(new RevealVotesCommand(id), ct);

        return result switch
        {
            RevealVotesResult.Success => StatusCode(201),
            RevealVotesResult.RoomNotFound => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/reset")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reset([FromRoute] int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var result = await _bus.InvokeAsync<ResetVotesResult>(new ResetVotesCommand(id), ct);

        return result switch
        {
            ResetVotesResult.Success => StatusCode(201),
            ResetVotesResult.RoomNotFound => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }

    [HttpPost("{id:int}/leave")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Leave([FromRoute] int id, [FromBody] LeaveRoomRequest request, CancellationToken ct)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var command = new LeaveRoomCommand(id, request.PlayerName);
        var result = await _bus.InvokeAsync<LeaveRoomResult>(command, ct);

        return result switch
        {
            LeaveRoomResult.Success => NoContent(),
            LeaveRoomResult.RoomNotFound => NotFound(),
            LeaveRoomResult.PlayerNotInRoom => NotFound(),
            _ => throw new InvalidOperationException($"Unknown result type: {result.GetType().Name}")
        };
    }
}
