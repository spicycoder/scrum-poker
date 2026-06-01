namespace ScrumPoker.Domain.Rooms.Events;

public sealed record VotesRevealed(int RoomId, Room Room) : IRoomState;
