using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.API.Features.Vote;
using ScrumPoker.Application.Features.Commands.Vote;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class VoteEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public VoteEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_201Created_When_Success()
    {
        var request = new VoteRequest("Alice", "5");
        var room = new Room { Id = 1, Players = [new Player("Alice", "5")] };
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.Success(room));

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.RoomNotFound());

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_PlayerNotInRoom()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.PlayerNotInRoom());

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var request = new VoteRequest("Bob", "5");

        var result = await _sut.Vote(id, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<VoteResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var request = new VoteRequest("Bob", "5");
        var unknownResult = Substitute.For<VoteResult>();
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Vote(1, request, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
