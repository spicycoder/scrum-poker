using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.API.Features.Common;
using ScrumPoker.Application.Features.Queries.GetGameState;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class GetRoomStateEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public GetRoomStateEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_200Ok_When_RoomExists()
    {
        var room = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _bus.InvokeAsync<Room?>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result = await _sut.Get(42, TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(200);

        var response = ok.Value.ShouldBeOfType<GameStateResponse>();
        response.GameId.ShouldBe(42);
        response.Players.ShouldHaveSingleItem();
        response.Players.ShouldContainKey("Alice");
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<Room?>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var result = await _sut.Get(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

}
