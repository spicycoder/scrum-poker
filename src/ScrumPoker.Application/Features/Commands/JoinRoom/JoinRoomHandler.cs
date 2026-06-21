using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.JoinRoom;

public sealed class JoinRoomHandler(IRoomRepository repository, IMessageBus bus, IStatsRepository stats)
{
    public async Task<Room?> Handle(JoinRoomCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null) return null;

        var (updated, @event) = room.Join(command.PlayerName);
        var saved = await repository.SaveAsync(updated, null, ct);
        await bus.PublishAsync(@event);

        if (!room.IsWarmup)
        {
            var monthKey = DateTime.UtcNow.ToString("yyyy-MM");
            await stats.RecordPlayerJoinedAsync(monthKey, ct);
        }

        return saved;
    }
}
