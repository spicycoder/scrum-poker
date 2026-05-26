namespace ScrumPoker.Application.Features.Vote;

public sealed record VoteCommand(int RoomId, string PlayerName, string Value);
