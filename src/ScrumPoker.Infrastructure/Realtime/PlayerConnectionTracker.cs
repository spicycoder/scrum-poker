using System.Collections.Concurrent;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PlayerConnectionTracker
{
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), ConcurrentDictionary<string, byte>> _connections = new();
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), CancellationTokenSource> _pendingRemovals = new();
    private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(3);

    public Func<int, string, Task>? PlayerRemoved { get; set; }

    public void Join(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);

        // Cancel pending removal if reconnecting within grace period
        if (_pendingRemovals.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

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
            var cts = new CancellationTokenSource();
            _pendingRemovals.TryAdd(key, cts);
            _ = RemoveAfterGracePeriod(key, cts.Token);
        }
    }

    private async Task RemoveAfterGracePeriod((int RoomId, string PlayerName) key, CancellationToken ct)
    {
        try
        {
            await Task.Delay(GracePeriod, ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _pendingRemovals.TryRemove(key, out _);
        _connections.TryRemove(key, out _);
        _ = PlayerRemoved?.Invoke(key.RoomId, key.PlayerName);
    }
}
