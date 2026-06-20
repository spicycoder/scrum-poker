using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using StackExchange.Redis;

namespace ScrumPoker.Persistence;

public sealed class RoomRepository(IConnectionMultiplexer redis, IOptions<GameSettings> settings) : IRoomRepository
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(settings.Value.ExpirationSeconds);

    public async Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(id.ToString());
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Room>(value.ToString());
    }

    public async Task<Room> SaveAsync(Room room, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var id = room.Id == 0 ? await GenerateIdAsync() : room.Id;
        var saved = room with { Id = id };

        await _db.StringSetAsync(id.ToString(), JsonSerializer.Serialize(saved), expiry ?? _ttl);
        return saved;
    }

    private async Task<int> GenerateIdAsync()
    {
        while (true)
        {
            var id = RandomNumberGenerator.GetInt32(1000, 10000);
            if (!await _db.KeyExistsAsync(id.ToString()))
            {
                return id;
            }
        }
    }
}
