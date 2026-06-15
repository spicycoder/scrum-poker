using Dapper;
using Npgsql;
using ScrumPoker.Application.Abstractions;

namespace ScrumPoker.Persistence.Stats;

public sealed class StatsRepository(NpgsqlDataSource dataSource) : IStatsRepository
{
    public async Task RecordGameCreatedAsync(string monthKey, CancellationToken ct = default)
    {
        using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("""
            INSERT INTO MonthlyStats (MonthKey, GameCount, PlayerCount, UpdatedAt)
            VALUES (@MonthKey, 1, 1, NOW())
            ON CONFLICT (MonthKey) DO UPDATE SET
                GameCount = MonthlyStats.GameCount + 1,
                PlayerCount = MonthlyStats.PlayerCount + 1,
                UpdatedAt = NOW()
            """, new { MonthKey = monthKey });
    }

    public async Task RecordPlayerJoinedAsync(string monthKey, CancellationToken ct = default)
    {
        using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("""
            INSERT INTO MonthlyStats (MonthKey, GameCount, PlayerCount, UpdatedAt)
            VALUES (@MonthKey, 0, 1, NOW())
            ON CONFLICT (MonthKey) DO UPDATE SET
                PlayerCount = MonthlyStats.PlayerCount + 1,
                UpdatedAt = NOW()
            """, new { MonthKey = monthKey });
    }

    public async Task<StatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        using var conn = await dataSource.OpenConnectionAsync(ct);
        var rows = (await conn.QueryAsync<MonthlyStatRow>(
            "SELECT MonthKey, GameCount, PlayerCount FROM MonthlyStats ORDER BY MonthKey", ct)).AsList();

        var current = rows.LastOrDefault() ?? new MonthlyStatRow(
            DateTime.UtcNow.ToString("yyyy-MM"), 0, 0);

        return new StatsResponse(rows, current);
    }
}
