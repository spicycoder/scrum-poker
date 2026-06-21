using System.Collections.Concurrent;

namespace ScrumPoker.Infrastructure.Realtime;

// ponytail: single-instance only. Add Redis backplane coordination if multi-instance needed.
public sealed class PlayerConnectionTracker
{
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), ConcurrentDictionary<string, byte>> _connections = new();
    private readonly object _lock = new();

    public Func<int, string, Task>? PlayerRemoved { get; set; }

    public Task Join(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);
        lock (_lock)
        {
            var set = _connections.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>());
            set.TryAdd(connectionId, 0);
        }
        return Task.CompletedTask;
    }

    public void Leave(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);
        bool invokeRemoved = false;
        lock (_lock)
        {
            if (!_connections.TryGetValue(key, out var set))
                return;

            set.TryRemove(connectionId, out _);

            if (set.IsEmpty)
            {
                _connections.TryRemove(key, out _);
                invokeRemoved = true;
            }
        }

        if (invokeRemoved)
            _ = PlayerRemoved?.Invoke(roomId, playerName);
    }
}
