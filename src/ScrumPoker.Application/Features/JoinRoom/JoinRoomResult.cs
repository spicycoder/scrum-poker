using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.JoinRoom;

public abstract record JoinRoomResult
{
    public sealed record Success(Room Room) : JoinRoomResult;
    public sealed record RoomNotFound : JoinRoomResult;
    public sealed record PlayerAlreadyInRoom : JoinRoomResult;
}
