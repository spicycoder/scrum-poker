using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.JoinRoom;

public sealed class JoinRoomHandler(IRoomRepository repository)
{
    public async Task<JoinRoomResult> Handle(JoinRoomCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new JoinRoomResult.RoomNotFound();
        }

        if (room.Players.Any(p => p.Name == command.PlayerName))
        {
            return new JoinRoomResult.PlayerAlreadyInRoom();
        }

        room = room with { Players = [.. room.Players, new Player(command.PlayerName, null)] };
        room = await repository.SaveAsync(room, ct);
        return new JoinRoomResult.Success(room);
    }
}
