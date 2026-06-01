namespace ScrumPoker.API.Features.CreateRoom;

public sealed class CreateRoomRequestValidatorTests
{
    private readonly CreateRoomRequestValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var request = new CreateRoomRequest(string.Empty, []);

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var request = new CreateRoomRequest(new string('A', 51), []);

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Pass_When_PlayerName_IsValid()
    {
        var request = new CreateRoomRequest("Alice", ["0", "1"]);

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeTrue();
    }
}
