namespace ScrumPoker.Domain.Abstractions;

public sealed record StatsResponse(List<MonthlyStatRow> MonthlyStats, MonthlyStatRow CurrentMonth);
