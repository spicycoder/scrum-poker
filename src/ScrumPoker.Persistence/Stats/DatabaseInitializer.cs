using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ScrumPoker.Persistence.Stats;

public sealed class DatabaseInitializer(NpgsqlDataSource dataSource) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var conn = await dataSource.OpenConnectionAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS MonthlyStats (
                MonthKey TEXT PRIMARY KEY,
                GameCount INT NOT NULL DEFAULT 0,
                PlayerCount INT NOT NULL DEFAULT 0,
                UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
            """;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
