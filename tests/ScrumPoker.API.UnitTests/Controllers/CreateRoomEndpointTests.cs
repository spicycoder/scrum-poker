using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.Application.Features.Commands.CreateRoom;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class CreateRoomEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public CreateRoomControllerTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_201Created_With_LocationHeader()
    {
        var request = new CreateRoomRequest("Alice");
        var room = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns(room);

        var result = await _sut.Create(request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
        _sut.Response.Headers.Location.ToString().ShouldBe("/api/rooms/42");
    }

    [Fact]
    public async Task Should_Pass_Command_To_Bus()
    {
        var request = new CreateRoomRequest("Alice");
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns(new Room());

        await _sut.Create(request, TestContext.Current.CancellationToken);

        await _bus.Received(1).InvokeAsync<Room>(
            Arg.Is<CreateRoomCommand>(c => c.PlayerName == "Alice"),
            Arg.Any<CancellationToken>(),
            Arg.Any<TimeSpan?>());
    }
}
