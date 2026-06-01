namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteCommandValidatorTests
{
    private readonly VoteCommandValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_RoomId_IsZero()
    {
        var command = new VoteCommand(0, "Bob", "5");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RoomId");
    }

    [Fact]
    public void Validate_Should_Fail_When_RoomId_IsNegative()
    {
        var command = new VoteCommand(-1, "Bob", "5");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RoomId");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var command = new VoteCommand(1, string.Empty, "5");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var command = new VoteCommand(1, new string('A', 51), "5");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_Value_IsEmpty()
    {
        var command = new VoteCommand(1, "Bob", string.Empty);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Value");
    }

    [Fact]
    public void Validate_Should_Pass_When_AllFields_AreValid()
    {
        var command = new VoteCommand(1, "Bob", "5");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

}
