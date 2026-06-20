using AspNetCore.Swagger.Themes;

using FluentValidation;
using FluentValidation.AspNetCore;

using ScrumPoker.API.Middleware;
using ScrumPoker.API.Settings;
using ScrumPoker.Application;
using ScrumPoker.Application.Features.Commands.LeaveRoom;
using ScrumPoker.Infrastructure;
using ScrumPoker.Infrastructure.Realtime;
using ScrumPoker.Persistence;
using Wolverine;

using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");
builder.AddNpgsqlDataSource("statsdb");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins is { Length: > 0 })
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("Location");
        });
    });
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(typeof(PokerHub).Assembly);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("redis"));
builder.Services.AddPersistence();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddOptions<WarmupSettings>()
    .BindConfiguration("Warmup");

var app = builder.Build();

var tracker = app.Services.GetRequiredService<PlayerConnectionTracker>();
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
tracker.PlayerRemoved = async (roomId, playerName) =>
{
    try
    {
        using var scope = scopeFactory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new LeaveRoomCommand(roomId, playerName));
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "PlayerRemoved cleanup failed for {PlayerName} in room {RoomId}", playerName, roomId);
    }
};

app.MapDefaultEndpoints();

app.UseExceptionHandler();

if (allowedOrigins is { Length: > 0 })
{
    app.UseCors();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(Theme.Futuristic, options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Scrum Poker API");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHub<PokerHub>("/api/hub");

app.Run();
