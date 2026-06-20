using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.Application.Features.Commands.JoinRoom;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class JoinRoomEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public JoinRoomEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_201Created_When_Success()
    {
        var request = new JoinRoomRequest("Bob");
        var room = new Room { Id = 1, Players = [new Player("Alice", null), new Player("Bob", null)] };
        _bus.InvokeAsync<Room?>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new JoinRoomRequest("Bob");
        _bus.InvokeAsync<Room?>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_409Conflict_When_PlayerAlreadyInRoom()
    {
        var request = new JoinRoomRequest("Alice");
        _bus.InvokeAsync<Room?>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Room?>(new InvalidOperationException("already in room")));

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ConflictResult>();
    }

}
