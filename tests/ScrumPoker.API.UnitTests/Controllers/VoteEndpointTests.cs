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
        _bus.InvokeAsync<Room?>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<Room?>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_PlayerNotInRoom()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<Room?>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Room?>(new InvalidOperationException("not in room")));

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

}
