using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.Rooms;

public sealed record Room
{
    public int Id { get; init; }
    public List<Player> Players { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public static (Room Room, RoomCreated Event) Create(string playerName)
    {
        var room = new Room { Players = [new Player(playerName, null)] };
        return (room, new RoomCreated(room.Id, playerName));
    }

    public (Room Room, PlayerJoined Event) Join(string playerName)
    {
        if (Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException($"Player '{playerName}' is already in room.");
        }

        var updated = this with { Players = [.. Players, new Player(playerName, null)] };
        return (updated, new PlayerJoined(Id, playerName));
    }

    public (Room Room, VoteCast Event) Vote(string playerName, string value)
    {
        if (!Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException($"Player '{playerName}' is not in room.");
        }

        var updated = this with
        {
            Players = Players
                .Select(p => p.Name == playerName ? p with { Value = value } : p)
                .ToList()
        };

        return (updated, new VoteCast(Id, playerName, value));
    }
}
