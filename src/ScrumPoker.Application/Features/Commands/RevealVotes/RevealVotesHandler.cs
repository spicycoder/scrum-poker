using ScrumPoker.Application.Abstractions;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public sealed class RevealVotesHandler(IRoomRepository repository)
{
    public async Task<RevealVotesResult> Handle(RevealVotesCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new RevealVotesResult.RoomNotFound();
        }

        var updated = room.Reveal();
        await repository.SaveAsync(updated, ct);
        return new RevealVotesResult.Success(updated);
    }
}
