namespace ScrumPoker.Application.Features.JoinRoom;

public sealed class JoinRoomCommandValidatorTests
{
    private readonly JoinRoomCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_RoomId_IsZero()
    {
        var command = new JoinRoomCommand(0, "Bob");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RoomId");
    }

    [Fact]
    public void Validate_Should_Fail_When_RoomId_IsNegative()
    {
        var command = new JoinRoomCommand(-1, "Bob");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RoomId");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var command = new JoinRoomCommand(1, string.Empty);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var command = new JoinRoomCommand(1, new string('A', 51));

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Pass_When_AllFields_AreValid()
    {
        var command = new JoinRoomCommand(1, "Bob");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
