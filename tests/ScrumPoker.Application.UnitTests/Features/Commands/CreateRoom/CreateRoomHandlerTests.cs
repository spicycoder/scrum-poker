using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.CreateRoom;

public sealed class CreateRoomHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly CreateRoomHandler _sut;

    public CreateRoomHandlerTests()
    {
        _sut = new CreateRoomHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_CreateRoom_WithPlayer()
    {
        var command = new CreateRoomCommand("Alice", ["0", "1"]);
        var savedRoom = new Room { Id = 1, Players = [new Player("Alice", null)] };
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<CancellationToken>()).Returns(savedRoom);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBe(savedRoom);
        result.Players.ShouldHaveSingleItem();
        result.Players[0].Name.ShouldBe("Alice");
        result.Players[0].Value.ShouldBeNull();

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Players.Count == 1 && r.Players[0].Name == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishRoomCreatedEvent()
    {
        var command = new CreateRoomCommand("Alice", ["0", "1"]);
        var savedRoom = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<CancellationToken>()).Returns(savedRoom);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(
            Arg.Is<RoomCreated>(e => e.RoomId == 42 && e.PlayerName == "Alice"));
    }
}
