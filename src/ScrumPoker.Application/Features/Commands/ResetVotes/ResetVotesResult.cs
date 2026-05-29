using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Commands.ResetVotes;

public abstract record ResetVotesResult
{
    public sealed record Success(Room Room) : ResetVotesResult;
    public sealed record RoomNotFound : ResetVotesResult;
}
