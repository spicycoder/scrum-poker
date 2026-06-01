namespace ScrumPoker.Application.Features.Commands.CreateRoom;

public sealed class CreateRoomCommandValidatorTests
{
    private readonly CreateRoomCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var command = new CreateRoomCommand(string.Empty, []);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var command = new CreateRoomCommand(new string('A', 51), []);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Pass_When_PlayerName_IsValid()
    {
        var command = new CreateRoomCommand("Alice", ["0", "1"]);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
