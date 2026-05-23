namespace ScrumPoker.API.Features.JoinRoom;

public sealed class JoinRoomRequestValidatorTests
{
    private readonly JoinRoomRequestValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var request = new JoinRoomRequest(string.Empty);

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var request = new JoinRoomRequest(new string('A', 51));

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Pass_When_PlayerName_IsValid()
    {
        var request = new JoinRoomRequest("Bob");

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeTrue();
    }
}
