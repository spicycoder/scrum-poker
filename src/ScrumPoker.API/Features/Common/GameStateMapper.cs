using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.API.Features.Common;

public static class GameStateMapper
{
    public static GameStateResponse ToResponse(Room room) =>
        new(room.Id, room.Players.ToDictionary(p => p.Name, p => p.Value), room.Revealed, room.CardSet);
}
