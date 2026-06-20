using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<Room?> Handle(VoteCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null) return null;

        var (updated, @event) = room.Vote(command.PlayerName, command.Value);
        var saved = await repository.SaveAsync(updated, null, ct);
        await bus.PublishAsync(@event);
        return saved;
    }
}
