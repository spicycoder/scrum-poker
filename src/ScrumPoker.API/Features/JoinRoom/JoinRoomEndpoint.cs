using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using ScrumPoker.API.Features.Common;
using ScrumPoker.Application.Features.JoinRoom;

namespace ScrumPoker.API.Features.JoinRoom;

public static class JoinRoomEndpoint
{
    [WolverinePost("/rooms/{id}/join")]
    public static async Task<IResult> Post([FromRoute] int id, [FromBody] JoinRoomRequest request, IMessageBus bus, CancellationToken ct)
    {
        var command = new JoinRoomCommand(id, request.PlayerName);
        var result = await bus.InvokeAsync<JoinRoomResult>(command, ct);

        return result switch
        {
            JoinRoomResult.Success(var room) => Results.Ok(GameStateMapper.ToResponse(room)),
            JoinRoomResult.RoomNotFound => Results.NotFound(),
            JoinRoomResult.PlayerAlreadyInRoom => Results.Conflict(),
            _ => Results.StatusCode(500)
        };
    }
}
