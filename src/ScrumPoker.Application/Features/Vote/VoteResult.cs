using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Vote;

public abstract record VoteResult
{
    public sealed record Success(Room Room) : VoteResult;
    public sealed record RoomNotFound : VoteResult;
    public sealed record PlayerNotInRoom : VoteResult;
}
