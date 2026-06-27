namespace ScrumPoker.Domain.Abstractions;

public interface IStatsRepository
{
    Task RecordGameCreatedAsync(string monthKey, CancellationToken ct = default);
    Task RecordPlayerJoinedAsync(string monthKey, CancellationToken ct = default);
    Task<StatsResponse> GetStatsAsync(CancellationToken ct = default);
}
