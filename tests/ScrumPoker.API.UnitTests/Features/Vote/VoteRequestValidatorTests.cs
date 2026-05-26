namespace ScrumPoker.API.Features.Vote;

public sealed class VoteRequestValidatorTests
{
    private readonly VoteRequestValidator _sut = new();

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_IsEmpty()
    {
        var request = new VoteRequest(string.Empty, "5");

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_PlayerName_ExceedsMaxLength()
    {
        var request = new VoteRequest(new string('A', 51), "5");

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PlayerName");
    }

    [Fact]
    public void Validate_Should_Fail_When_Value_IsEmpty()
    {
        var request = new VoteRequest("Bob", string.Empty);

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Value");
    }

    [Fact]
    public void Validate_Should_Fail_When_Value_IsInvalid()
    {
        var request = new VoteRequest("Bob", "abc");

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Value");
    }

    [Fact]
    public void Validate_Should_Pass_When_AllFields_AreValid()
    {
        var request = new VoteRequest("Bob", "5");

        var result = _sut.Validate(request);

        result.IsValid.ShouldBeTrue();
    }
}
