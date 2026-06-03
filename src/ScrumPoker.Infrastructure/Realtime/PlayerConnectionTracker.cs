using System.Collections.Concurrent;
using StackExchange.Redis;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PlayerConnectionTracker(IConnectionMultiplexer redis)
{
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), ConcurrentDictionary<string, byte>> _connections = new();
    private readonly ConcurrentDictionary<(int RoomId, string PlayerName), CancellationTokenSource> _pendingRemovals = new();
    private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PendingRemovalTtl = GracePeriod + TimeSpan.FromSeconds(10);

    public Func<int, string, Task>? PlayerRemoved { get; set; }

    public async Task Join(int roomId, string playerName, string connectionId)
    {
        var key = (roomId, playerName);

        // Cancel local pending removal if reconnecting on same instance
        if (_pendingRemovals.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        // Cross-instance: delete Redis key so grace timer on another instance skips removal
        try
        {
            await redis.GetDatabase().KeyDeleteAsync(PendingRemovalKey(roomId, playerName));
        }
        catch { /* Redis unavailable — cross-instance cancel won't work, local cancel still does */ }

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
            if (_pendingRemovals.TryAdd(key, cts))
            {
                // Cross-instance: store pending removal in Redis with TTL matching grace period
                _ = SetPendingRemovalAsync(roomId, playerName);
                _ = RemoveAfterGracePeriod(key, cts.Token);
            }
            else
            {
                cts.Dispose();
            }
        }
    }

    private async Task SetPendingRemovalAsync(int roomId, string playerName)
    {
        try
        {
            await redis.GetDatabase().StringSetAsync(PendingRemovalKey(roomId, playerName), "1", PendingRemovalTtl);
        }
        catch { /* Redis unavailable — cross-instance cancel won't work, local cancel still does */ }
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

        // Cross-instance: atomically delete the key.
        // Returns true  → key existed, we own the removal, proceed.
        // Returns false → another instance's JoinRoom already deleted it, skip removal.
        try
        {
            var deleted = await redis.GetDatabase().KeyDeleteAsync(PendingRemovalKey(key.RoomId, key.PlayerName));
            if (!deleted)
                return;
        }
        catch { /* Redis unavailable — proceed with removal */ }

        _pendingRemovals.TryRemove(key, out _);
        _connections.TryRemove(key, out _);
        _ = PlayerRemoved?.Invoke(key.RoomId, key.PlayerName);
    }

    private static string PendingRemovalKey(int roomId, string playerName) =>
        $"pending-removal:{roomId}:{playerName}";
}
