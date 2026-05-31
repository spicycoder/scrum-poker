using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.Vote;
using ScrumPoker.Application.Features.Commands.CreateRoom;
using ScrumPoker.Application.Features.Commands.JoinRoom;
using ScrumPoker.Application.Features.Commands.ResetVotes;
using ScrumPoker.Application.Features.Commands.RevealVotes;
using ScrumPoker.Application.Features.Commands.Vote;
using ScrumPoker.Application.Features.Queries.GetGameState;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.Controllers;

public sealed class RoomControllerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public RoomControllerTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Create_Should_Return_201Created_With_LocationHeader()
    {
        var request = new CreateRoomRequest("Alice");
        var room = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>())
            .Returns(room);

        var result = await _sut.Create(request, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);

        _sut.Response.Headers.Location.ToString().ShouldBe("/api/rooms/42");

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
        _bus.InvokeAsync<Room>(Arg.Any<CreateRoomCommand>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan?>())
            .Returns(new Room());

        await _sut.Create(request, ct);

        await _bus.Received(1).InvokeAsync<Room>(
            Arg.Any<CreateRoomCommand>(),
            ct,
            Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task Join_Should_Return_201Created_When_Success()
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

    [Fact]
    public async Task Vote_Should_Return_201Created_When_Success()
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
    public async Task Vote_Should_Return_404NotFound_When_RoomNotFound()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.RoomNotFound());

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Vote_Should_Return_404NotFound_When_PlayerNotInRoom()
    {
        var request = new VoteRequest("Bob", "5");
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.PlayerNotInRoom());

        var result = await _sut.Vote(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Vote_Should_Return_400BadRequest_When_IdIsZero()
    {
        var request = new VoteRequest("Bob", "5");

        var result = await _sut.Vote(0, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<VoteResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Vote_Should_Return_400BadRequest_When_IdIsNegative()
    {
        var request = new VoteRequest("Bob", "5");

        var result = await _sut.Vote(-5, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<VoteResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Vote_Should_Pass_CancellationToken()
    {
        var request = new VoteRequest("Alice", "5");
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(new VoteResult.Success(new Room()));

        await _sut.Vote(1, request, ct);

        await _bus.Received(1).InvokeAsync<VoteResult>(
            Arg.Is<VoteCommand>(c => c.RoomId == 1 && c.PlayerName == "Alice" && c.Value == "5"),
            ct);
    }

    [Fact]
    public async Task Vote_Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var request = new VoteRequest("Bob", "5");
        var unknownResult = Substitute.For<VoteResult>();
        _bus.InvokeAsync<VoteResult>(Arg.Any<VoteCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Vote(1, request, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }

    [Fact]
    public async Task Get_Should_Return_200Ok_When_RoomExists()
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
    public async Task Get_Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<GetGameStateResult>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetGameStateResult.RoomNotFound());

        var result = await _sut.Get(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Get_Should_Return_400BadRequest_When_IdIsZero()
    {
        var result = await _sut.Get(0, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<GetGameStateResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Get_Should_Return_400BadRequest_When_IdIsNegative()
    {
        var result = await _sut.Get(-5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<GetGameStateResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Get_Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<GetGameStateResult>();
        _bus.InvokeAsync<GetGameStateResult>(Arg.Any<GetGameStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Get(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }

    [Fact]
    public async Task Reveal_Should_Return_201Created_When_Success()
    {
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RevealVotesResult.Success(new Room()));

        var result = await _sut.Reveal(1, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Reveal_Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RevealVotesResult.RoomNotFound());

        var result = await _sut.Reveal(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Reveal_Should_Return_400BadRequest_When_IdIsZero()
    {
        var result = await _sut.Reveal(0, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<RevealVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Reveal_Should_Return_400BadRequest_When_IdIsNegative()
    {
        var result = await _sut.Reveal(-5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<RevealVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Reveal_Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<RevealVotesResult>();
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Reveal(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }

    [Fact]
    public async Task Reset_Should_Return_201Created_When_Success()
    {
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ResetVotesResult.Success(new Room()));

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Reset_Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ResetVotesResult.RoomNotFound());

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Reset_Should_Return_400BadRequest_When_IdIsZero()
    {
        var result = await _sut.Reset(0, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<ResetVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Reset_Should_Return_400BadRequest_When_IdIsNegative()
    {
        var result = await _sut.Reset(-5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<ResetVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Reset_Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<ResetVotesResult>();
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Reset(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
