using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.API.Features.LeaveRoom;
using ScrumPoker.Application.Features.Commands.LeaveRoom;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class LeaveRoomEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public LeaveRoomEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_204NoContent_When_Success()
    {
        var request = new LeaveRoomRequest("Bob");
        var room = new Room { Id = 1, Players = [new Player("Alice", null)] };
        _bus.InvokeAsync<Room?>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new LeaveRoomRequest("Bob");
        _bus.InvokeAsync<Room?>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_PlayerNotInRoom()
    {
        var request = new LeaveRoomRequest("Bob");
        _bus.InvokeAsync<Room?>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Room?>(new InvalidOperationException("not in room")));

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

}
