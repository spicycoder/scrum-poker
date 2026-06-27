namespace ScrumPoker.AppHost.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class WarmupCollection : ICollectionFixture<WarmupFixture>
{
    public const string Name = "Warmup";
}
