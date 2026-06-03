using AspNetCore.Swagger.Themes;

using FluentValidation;
using FluentValidation.AspNetCore;

using ScrumPoker.API.Middleware;
using ScrumPoker.Application;
using ScrumPoker.Application.Features.Commands.LeaveRoom;
using ScrumPoker.Infrastructure;
using ScrumPoker.Infrastructure.Realtime;
using ScrumPoker.Persistence;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");

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

var app = builder.Build();

var tracker = app.Services.GetRequiredService<PlayerConnectionTracker>();
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
tracker.PlayerRemoved = (roomId, playerName) =>
{
    using var scope = scopeFactory.CreateScope();
    var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
    return bus.InvokeAsync(new LeaveRoomCommand(roomId, playerName));
};

app.MapDefaultEndpoints();

app.UseExceptionHandler();

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

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapControllers();
app.MapHub<PokerHub>("/api/hub");

app.Run();
