namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed record VoteCommand(int RoomId, string PlayerName, string Value);
