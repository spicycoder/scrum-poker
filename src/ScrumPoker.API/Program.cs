using AspNetCore.Swagger.Themes;
using ScrumPoker.Application;
using ScrumPoker.Infrastructure;
using ScrumPoker.Infrastructure.Realtime;
using ScrumPoker.Persistence;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(Theme.Futuristic, options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Scrum Poker API");
    });
}

app.UseHttpsRedirection();

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

app.MapHub<PokerHub>("/hub");

app.Run();
