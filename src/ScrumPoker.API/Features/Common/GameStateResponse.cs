namespace ScrumPoker.API.Features.Common;

public sealed record GameStateResponse(int GameId, List<GamePlayerResponse> Players, bool Revealed);
