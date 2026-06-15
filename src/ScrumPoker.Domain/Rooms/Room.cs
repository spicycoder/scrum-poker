using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.Rooms;

public sealed record Room
{
    public int Id { get; init; }
    public List<Player> Players { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool Revealed { get; init; }
    public bool IsWarmup { get; init; }
    public List<string> CardSet { get; init; } = [];

    public static (Room Room, RoomCreated Event) Create(string playerName, List<string>? cardSet = null)
    {
        var room = new Room { Players = [new Player(playerName, null)], CardSet = cardSet ?? [] };
        return (room, new RoomCreated(room.Id, playerName, room));
    }

    public (Room Room, PlayerJoined Event) Join(string playerName)
    {
        if (Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException($"Player '{playerName}' is already in room.");
        }

        var updated = this with { Players = [.. Players, new Player(playerName, null)] };
        return (updated, new PlayerJoined(Id, playerName, updated));
    }

    public (Room Room, VoteCast Event) Vote(string playerName, string value)
    {
        if (!Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException($"Player '{playerName}' is not in room.");
        }

        var updatedPlayers = Players
            .Select(p => p.Name == playerName ? p with { Value = value } : p)
            .ToList();

        var updated = this with
        {
            Players = updatedPlayers,
            Revealed = updatedPlayers.All(p => p.Value is not null)
        };

        return (updated, new VoteCast(Id, playerName, value, updated));
    }

    public (Room Room, VotesRevealed Event) Reveal()
    {
        var updated = this with { Revealed = true };
        return (updated, new VotesRevealed(Id, updated));
    }

    public (Room Room, VotesReset Event) ResetVotes()
    {
        var updated = this with
        {
            Players = Players.Select(p => p with { Value = null }).ToList(),
            Revealed = false
        };
        return (updated, new VotesReset(Id, updated));
    }

    public (Room Room, PlayerLeft Event) Leave(string playerName)
    {
        if (!Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException($"Player '{playerName}' is not in room.");
        }

        var updated = this with
        {
            Players = Players.Where(p => p.Name != playerName).ToList()
        };
        return (updated, new PlayerLeft(Id, playerName, updated));
    }
}
