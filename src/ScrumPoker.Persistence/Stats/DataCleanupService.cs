using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ScrumPoker.Persistence.Stats;

public sealed class DataCleanupService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        while (await timer.WaitForNextTickAsync(ct))
        {
            using var scope = scopeFactory.CreateScope();
            var ds = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
            using var conn = await ds.OpenConnectionAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MonthlyStats WHERE UpdatedAt < CURRENT_TIMESTAMP - INTERVAL '6 months'";
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
