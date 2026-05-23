using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.Application.Features.CreateRoom;
using ScrumPoker.Application.Features.JoinRoom;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.Controllers;

public sealed class RoomControllerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public RoomControllerTests()
    {
        _sut = new RoomController(_bus);
    }

    [Fact]
    public async Task Create_Should_Return_201Created_With_GameStateResponse()
    {
        var request = new CreateRoomRequest("Alice");
        var room = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result =        await _sut.Create(request, TestContext.Current.CancellationToken);

        var created = result.ShouldBeOfType<CreatedResult>();
        created.StatusCode.ShouldBe(201);
        created.Location.ShouldBe("/rooms/42");

        var response = created.Value.ShouldBeOfType<GameStateResponse>();
        response.GameId.ShouldBe(42);
        response.Players.ShouldHaveSingleItem();
        response.Players[0].Name.ShouldBe("Alice");
        response.Players[0].Value.ShouldBeNull();

        await _bus.Received(1).InvokeAsync<Room>(
            Arg.Is<CreateRoomCommand>(c => c.PlayerName == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_Pass_CancellationToken()
    {
        var request = new CreateRoomRequest("Bob");
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new Room());

        await _sut.Create(request, ct);

        await _bus.Received(1).InvokeAsync<Room>(
            Arg.Any<CreateRoomCommand>(),
            ct);
    }

    [Fact]
    public async Task Join_Should_Return_200Ok_When_Success()
    {
        var request = new JoinRoomRequest("Bob");
        var room = new Room { Id = 1, Players = [new Player("Alice", null), new Player("Bob", null)] };
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.Success(room));

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(200);

        var response = ok.Value.ShouldBeOfType<GameStateResponse>();
        response.GameId.ShouldBe(1);
        response.Players.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Join_Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new JoinRoomRequest("Bob");
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.RoomNotFound());

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Join_Should_Return_409Conflict_When_PlayerAlreadyInRoom()
    {
        var request = new JoinRoomRequest("Alice");
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.PlayerAlreadyInRoom());

        var result = await _sut.Join(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ConflictResult>();
    }

    [Fact]
    public async Task Join_Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var request = new JoinRoomRequest("Bob");
        var unknownResult = Substitute.For<JoinRoomResult>();
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Join(1, request, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }

    [Fact]
    public async Task Join_Should_Return_400BadRequest_When_IdIsZero()
    {
        var request = new JoinRoomRequest("Bob");

        var result = await _sut.Join(0, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<JoinRoomResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Join_Should_Return_400BadRequest_When_IdIsNegative()
    {
        var request = new JoinRoomRequest("Bob");

        var result = await _sut.Join(-5, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<JoinRoomResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Join_Should_Pass_CancellationToken()
    {
        var request = new JoinRoomRequest("Bob");
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        _bus.InvokeAsync<JoinRoomResult>(Arg.Any<JoinRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResult.Success(new Room()));

        await _sut.Join(1, request, ct);

        await _bus.Received(1).InvokeAsync<JoinRoomResult>(
            Arg.Is<JoinRoomCommand>(c => c.RoomId == 1 && c.PlayerName == "Bob"),
            ct);
    }
}
