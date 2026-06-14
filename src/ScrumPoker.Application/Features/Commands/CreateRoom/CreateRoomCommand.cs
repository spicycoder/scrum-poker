namespace ScrumPoker.Application.Features.Commands.CreateRoom;

public sealed record CreateRoomCommand(string PlayerName, List<string> CardSet, TimeSpan? Expiry = null);
