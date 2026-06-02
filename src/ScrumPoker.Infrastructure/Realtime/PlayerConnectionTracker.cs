using System.Collections.Concurrent;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PlayerConnectionTracker
{
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), ConcurrentDictionary<string, byte>> _connections = new();
    public Func<int, string, Task>? PlayerRemoved { get; set; }

    public void Join(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);
        var set = _connections.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>());
        set.TryAdd(connectionId, 0);
    }

    public void Leave(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);
        if (!_connections.TryGetValue(key, out var set))
            return;

        set.TryRemove(connectionId, out _);

        if (set.IsEmpty)
        {
            _connections.TryRemove(key, out _);
            _ = PlayerRemoved?.Invoke(roomId, playerName);
        }
    }
}
