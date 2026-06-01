namespace ScrumPoker.Domain.Rooms.Events;

public sealed record VoteCast(int RoomId, string PlayerName, string Value, Room Room) : IRoomState;
