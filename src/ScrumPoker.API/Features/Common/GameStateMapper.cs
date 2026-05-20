using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.API.Features.Common;

public static class GameStateMapper
{
    public static GameStateResponse ToResponse(Room room) =>
        new(room.Id, room.Players.Select(p => new GamePlayerResponse(p.Name, p.Value)).ToList());
}
