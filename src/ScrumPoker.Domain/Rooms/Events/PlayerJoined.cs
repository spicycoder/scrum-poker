namespace ScrumPoker.Domain.Rooms.Events;

public sealed record PlayerJoined(int RoomId, string PlayerName, Room Room) : IRoomState;
