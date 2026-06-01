namespace ScrumPoker.Domain.Rooms.Events;

public sealed record RoomCreated(int RoomId, string PlayerName, Room Room) : IRoomState;
