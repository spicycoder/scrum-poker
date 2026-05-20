using Wolverine;
using Wolverine.Http;
using ScrumPoker.API.Features.Common;
using ScrumPoker.Application.Features.CreateRoom;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.API.Features.CreateRoom;

public static class CreateRoomEndpoint
{
    [WolverinePost("/rooms")]
    public static async Task<IResult> Post(CreateRoomRequest request, IMessageBus bus, CancellationToken ct)
    {
        var command = new CreateRoomCommand(request.PlayerName);
        var room = await bus.InvokeAsync<Room>(command, ct);
        return Results.Created($"/rooms/{room.Id}", GameStateMapper.ToResponse(room));
    }
}
