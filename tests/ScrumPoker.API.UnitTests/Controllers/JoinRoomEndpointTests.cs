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

    public JoinRoomControllerTests()
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
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.Success(room));

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new JoinRoomRequest("Bob");
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.RoomNotFound());

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_Return_409Conflict_When_PlayerAlreadyInRoom()
    {
        var request = new JoinRoomRequest("Alice");
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.PlayerAlreadyInRoom());

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ConflictResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var request = new JoinRoomRequest("Bob");

        var result = await _sut.Join(id, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<JoinRoomResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var request = new JoinRoomRequest("Bob");
        var unknownResult = Substitute.For<JoinRoomResult>();
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Join(1, request, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
