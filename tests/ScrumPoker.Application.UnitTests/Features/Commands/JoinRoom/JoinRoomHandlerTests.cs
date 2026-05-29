using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.JoinRoom;

public sealed class JoinRoomHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly JoinRoomHandler _sut;

    public JoinRoomHandlerTests()
    {
        _sut = new JoinRoomHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_Return_RoomNotFound_When_Room_DoesNotExist()
    {
        var command = new JoinRoomCommand(42, "Bob");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<JoinRoomResult.RoomNotFound>();
    }

    [Fact]
    public async Task Handle_Should_Return_PlayerAlreadyInRoom_When_Player_Exists()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Bob", null)]
        };
        var command = new JoinRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<JoinRoomResult.PlayerAlreadyInRoom>();
    }

    [Fact]
    public async Task Handle_Should_Return_Success_With_UpdatedRoom()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new JoinRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        var updatedRoom = room with { Players = [.. room.Players, new Player("Bob", null)] };
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<CancellationToken>()).Returns(updatedRoom);

        var result = await _sut.Handle(command, CancellationToken.None);

        var success = result.ShouldBeOfType<JoinRoomResult.Success>();
        success.Room.Players.Count.ShouldBe(2);
        success.Room.Players.ShouldContain(p => p.Name == "Alice");
        success.Room.Players.ShouldContain(p => p.Name == "Bob");

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Players.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishPlayerJoinedEvent_OnSuccess()
    {
        var room = new Room { Id = 1, Players = [new Player("Alice", null)] };
        var command = new JoinRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(
            Arg.Is<PlayerJoined>(e => e.RoomId == 1 && e.PlayerName == "Bob"));
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_RoomNotFound()
    {
        var command = new JoinRoomCommand(42, "Bob");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<PlayerJoined>());
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_PlayerAlreadyInRoom()
    {
        var room = new Room { Id = 1, Players = [new Player("Bob", null)] };
        var command = new JoinRoomCommand(1, "Bob");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<PlayerJoined>());
    }
}
