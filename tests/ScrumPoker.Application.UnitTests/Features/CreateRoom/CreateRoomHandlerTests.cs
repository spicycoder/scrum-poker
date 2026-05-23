using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.CreateRoom;

public sealed class CreateRoomHandlerTests
{
    [Fact]
    public async Task Handle_Should_CreateRoom_WithPlayer()
    {
        var repository = Substitute.For<IRoomRepository>();
        var handler = new CreateRoomHandler(repository);
        var command = new CreateRoomCommand("Alice");
        var savedRoom = new Room { Id = 1, Players = [new Player("Alice", null)] };
        repository.SaveAsync(Arg.Any<Room>(), Arg.Any<CancellationToken>()).Returns(savedRoom);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldBe(savedRoom);
        result.Players.ShouldHaveSingleItem();
        result.Players[0].Name.ShouldBe("Alice");
        result.Players[0].Value.ShouldBeNull();

        await repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Players.Count == 1 && r.Players[0].Name == "Alice"),
            Arg.Any<CancellationToken>());
    }
}
