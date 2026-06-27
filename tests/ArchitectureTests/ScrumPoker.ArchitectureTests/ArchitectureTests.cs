using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScrumPoker.ArchitectureTests;

public sealed class LayerArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(ScrumPoker.Domain.Abstractions.IStatsRepository).Assembly,
            typeof(ScrumPoker.Application.Bootstrap).Assembly,
            typeof(ScrumPoker.Infrastructure.Bootstrap).Assembly,
            typeof(ScrumPoker.Persistence.Bootstrap).Assembly,
            typeof(ScrumPoker.API.Controllers.RoomController).Assembly
        )
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInNamespace("ScrumPoker.Domain").As("Domain Layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInNamespace("ScrumPoker.Application").As("Application Layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInNamespace("ScrumPoker.Infrastructure").As("Infrastructure Layer");

    private static readonly IObjectProvider<IType> PersistenceLayer =
        Types().That().ResideInNamespace("ScrumPoker.Persistence").As("Persistence Layer");

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Persistence()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(PersistenceLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Persistence()
    {
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(PersistenceLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Application()
    {
        Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Persistence_Should_Not_Depend_On_Application()
    {
        Types().That().Are(PersistenceLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Persistence()
    {
        Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(PersistenceLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Persistence_Should_Not_Depend_On_Infrastructure()
    {
        Types().That().Are(PersistenceLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }
}
