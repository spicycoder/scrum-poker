using AspNetCore.Swagger.Themes;

using FluentValidation;
using FluentValidation.AspNetCore;

using ScrumPoker.Application;
using ScrumPoker.Infrastructure;
using ScrumPoker.Infrastructure.Realtime;
using ScrumPoker.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(Theme.Futuristic, options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Scrum Poker API");
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<PokerHub>("/hub");

app.Run();
