namespace ScrumPoker.Domain.Rooms.Events;

public sealed record PlayerLeft(int RoomId, string PlayerName, Room Room) : IRoomState;
