var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var statsDb = postgres.AddDatabase("statsdb");

var api =builder.AddProject<Projects.ScrumPoker_API>("scrumpoker-api")
    .WithReference(redis)
    .WithReference(statsDb)
    .WaitFor(redis)
    .WaitFor(postgres);

var ui = builder
    .AddViteApp("web", "../../web")
    .WithPnpm()
    .WaitFor(api)
    .WithReference(api);

builder.Build().Run();
