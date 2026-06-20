using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.LeaveRoom;

public sealed class LeaveRoomHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<Room?> Handle(LeaveRoomCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null) return null;

        var (updated, @event) = room.Leave(command.PlayerName);
        var saved = await repository.SaveAsync(updated, null, ct);
        await bus.PublishAsync(@event);
        return saved;
    }
}
