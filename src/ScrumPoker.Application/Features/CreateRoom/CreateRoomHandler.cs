using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.CreateRoom;

public sealed class CreateRoomHandler(IRoomRepository repository)
{
    public async Task<Room> Handle(CreateRoomCommand command, CancellationToken ct)
    {
        var room = new Room
        {
            Players = [new Player(command.PlayerName, null)]
        };

        return await repository.SaveAsync(room, ct);
    }
}
