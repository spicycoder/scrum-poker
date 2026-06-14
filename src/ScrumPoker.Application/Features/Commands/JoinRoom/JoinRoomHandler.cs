using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.JoinRoom;

public sealed class JoinRoomHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<JoinRoomResult> Handle(JoinRoomCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new JoinRoomResult.RoomNotFound();
        }

        try
        {
            var (updated, @event) = room.Join(command.PlayerName);
            var saved = await repository.SaveAsync(updated, null, ct);
            await bus.PublishAsync(@event);
            return new JoinRoomResult.Success(saved);
        }
        catch (InvalidOperationException)
        {
            return new JoinRoomResult.PlayerAlreadyInRoom();
        }
    }
}
