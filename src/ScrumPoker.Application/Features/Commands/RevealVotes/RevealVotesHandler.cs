using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public sealed class RevealVotesHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<Room?> Handle(RevealVotesCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null) return null;

        var (updated, @event) = room.Reveal();
        var saved = await repository.SaveAsync(updated, null, ct);
        await bus.PublishAsync(@event);
        return saved;
    }
}
