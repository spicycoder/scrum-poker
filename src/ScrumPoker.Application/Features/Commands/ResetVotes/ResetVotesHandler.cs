using ScrumPoker.Application.Abstractions;

namespace ScrumPoker.Application.Features.Commands.ResetVotes;

public sealed class ResetVotesHandler(IRoomRepository repository)
{
    public async Task<ResetVotesResult> Handle(ResetVotesCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new ResetVotesResult.RoomNotFound();
        }

        var updated = room.ResetVotes();
        await repository.SaveAsync(updated, ct);
        return new ResetVotesResult.Success(updated);
    }
}
