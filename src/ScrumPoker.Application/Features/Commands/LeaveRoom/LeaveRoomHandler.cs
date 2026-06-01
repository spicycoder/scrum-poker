using ScrumPoker.Application.Abstractions;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.LeaveRoom;

public sealed class LeaveRoomHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<LeaveRoomResult> Handle(LeaveRoomCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new LeaveRoomResult.RoomNotFound();
        }

        try
        {
            var (updated, @event) = room.Leave(command.PlayerName);
            await repository.SaveAsync(updated, ct);
            await bus.PublishAsync(@event);
            return new LeaveRoomResult.Success(updated);
        }
        catch (InvalidOperationException)
        {
            return new LeaveRoomResult.PlayerNotInRoom();
        }
    }
}
