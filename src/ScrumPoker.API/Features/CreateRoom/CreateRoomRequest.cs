namespace ScrumPoker.API.Features.CreateRoom;

public sealed record CreateRoomRequest(string PlayerName, List<string> CardSet);
