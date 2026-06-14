using ScrumPoker.Domain.Abstractions;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.ResetVotes;

public sealed class ResetVotesHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<ResetVotesResult> Handle(ResetVotesCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new ResetVotesResult.RoomNotFound();
        }

        var (updated, @event) = room.ResetVotes();
        await repository.SaveAsync(updated, null, ct);
        await bus.PublishAsync(@event);
        return new ResetVotesResult.Success(updated);
    }
}
