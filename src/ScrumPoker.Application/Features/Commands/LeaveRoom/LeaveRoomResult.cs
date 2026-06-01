using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Commands.LeaveRoom;

public abstract record LeaveRoomResult
{
    public sealed record Success(Room Room) : LeaveRoomResult;
    public sealed record RoomNotFound : LeaveRoomResult;
    public sealed record PlayerNotInRoom : LeaveRoomResult;
}
