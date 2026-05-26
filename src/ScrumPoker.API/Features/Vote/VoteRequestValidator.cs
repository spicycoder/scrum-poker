using FluentValidation;

namespace ScrumPoker.API.Features.Vote;

public sealed class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    private static readonly HashSet<string> AllowedValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "0", "0.5", "1", "2", "3", "5", "8", "13", "20", "?"
    };

    public VoteRequestValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .NotEmpty()
            .Must(v => AllowedValues.Contains(v))
                .WithMessage($"Value must be one of: {string.Join(", ", AllowedValues)}");
    }
}
