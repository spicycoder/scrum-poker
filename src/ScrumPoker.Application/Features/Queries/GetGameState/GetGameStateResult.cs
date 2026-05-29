using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Queries.GetGameState;

public abstract record GetGameStateResult
{
    public sealed record Success(Room Room) : GetGameStateResult;
    public sealed record RoomNotFound : GetGameStateResult;
}
