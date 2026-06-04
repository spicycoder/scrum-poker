using ScrumPoker.Domain.Abstractions;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public sealed class RevealVotesHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<RevealVotesResult> Handle(RevealVotesCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new RevealVotesResult.RoomNotFound();
        }

        var (updated, @event) = room.Reveal();
        await repository.SaveAsync(updated, ct);
        await bus.PublishAsync(@event);
        return new RevealVotesResult.Success(updated);
    }
}
