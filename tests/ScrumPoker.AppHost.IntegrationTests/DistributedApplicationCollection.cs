namespace ScrumPoker.AppHost.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class DistributedApplicationCollection
    : ICollectionFixture<DistributedApplicationFixture>
    , ICollectionFixture<ShortTtlDistributedApplicationFixture>
{
    public const string Name = "Distributed application";
}
