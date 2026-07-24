var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var api = builder.AddProject<Projects.ScrumPoker_API>("scrumpoker-api")
    .WithReference(redis)
    .WaitFor(redis);

var ui = builder
    .AddViteApp("web", "../../web")
    .WithPnpm()
    .WaitFor(api)
    .WithReference(api);

builder.Build().Run();
