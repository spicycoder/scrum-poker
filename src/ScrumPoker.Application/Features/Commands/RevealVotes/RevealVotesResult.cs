using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public abstract record RevealVotesResult
{
    public sealed record Success(Room Room) : RevealVotesResult;
    public sealed record RoomNotFound : RevealVotesResult;
}
