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

    public GetRoomStateControllerTests()
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
        _bus.InvokeAsync<GetGameStateResult>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetGameStateResult.Success(room));

        var result = await _sut.Get(42, TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(200);

        var response = ok.Value.ShouldBeOfType<GameStateResponse>();
        response.GameId.ShouldBe(42);
        response.Players.ShouldHaveSingleItem();
        response.Players[0].Name.ShouldBe("Alice");
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<GetGameStateResult>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetGameStateResult.RoomNotFound());

        var result = await _sut.Get(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var result = await _sut.Get(id, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<GetGameStateResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<GetGameStateResult>();
        _bus.InvokeAsync<GetGameStateResult>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Get(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
