namespace ScrumPoker.Domain.Rooms.Events;

public sealed record VotesReset(int RoomId, Room Room) : IRoomState;
