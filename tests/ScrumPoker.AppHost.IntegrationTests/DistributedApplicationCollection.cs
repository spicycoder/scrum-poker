namespace ScrumPoker.AppHost.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class DistributedApplicationCollection
    : ICollectionFixture<DistributedApplicationFixture>
{
    public const string Name = "Distributed application";
}
