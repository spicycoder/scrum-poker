namespace ScrumPoker.Application.Abstractions;

public sealed record StatsResponse(List<MonthlyStatRow> MonthlyStats, MonthlyStatRow CurrentMonth);
