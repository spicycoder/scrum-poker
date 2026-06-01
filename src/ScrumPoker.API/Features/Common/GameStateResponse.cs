namespace ScrumPoker.API.Features.Common;

public sealed record GameStateResponse(int GameId, Dictionary<string, string?> Players, bool Revealed);
