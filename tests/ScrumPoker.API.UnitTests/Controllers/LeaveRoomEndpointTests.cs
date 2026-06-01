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
        _bus.InvokeAsync<LeaveRoomResult>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new LeaveRoomResult.Success(room));

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new LeaveRoomRequest("Bob");
        _bus.InvokeAsync<LeaveRoomResult>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new LeaveRoomResult.RoomNotFound());

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_PlayerNotInRoom()
    {
        var request = new LeaveRoomRequest("Bob");
        _bus.InvokeAsync<LeaveRoomResult>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new LeaveRoomResult.PlayerNotInRoom());

        var result = await _sut.Leave(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var request = new LeaveRoomRequest("Bob");

        var result = await _sut.Leave(id, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<LeaveRoomResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var request = new LeaveRoomRequest("Bob");
        var unknownResult = Substitute.For<LeaveRoomResult>();
        _bus.InvokeAsync<LeaveRoomResult>(Arg.Any<LeaveRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Leave(1, request, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
