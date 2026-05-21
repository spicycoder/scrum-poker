var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

builder.AddProject<Projects.ScrumPoker_API>("scrumpoker-api")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
