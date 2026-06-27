using NSubstitute;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.LeaveRoom;

public sealed class LeaveRoomHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly LeaveRoomHandler _sut;

    public LeaveRoomHandlerTests()
    {
        _sut = new LeaveRoomHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Room_DoesNotExist()
    {
        var command = new LeaveRoomCommand(42, "Bob");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Player_NotInRoom()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new LeaveRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Return_Room_When_Valid()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null), new Player("Bob", null)]
        };
        var command = new LeaveRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Players.ShouldHaveSingleItem();
        result.Players[0].Name.ShouldBe("Alice");

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Players.Count == 1 && r.Players[0].Name == "Alice"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishPlayerLeftEvent_OnSuccess()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null), new Player("Bob", null)]
        };
        var command = new LeaveRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(
            Arg.Is<PlayerLeft>(e => e.RoomId == 1 && e.PlayerName == "Bob"));
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_RoomNotFound()
    {
        var command = new LeaveRoomCommand(42, "Bob");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<PlayerLeft>());
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_PlayerNotInRoom()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new LeaveRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<PlayerLeft>());
    }
}
