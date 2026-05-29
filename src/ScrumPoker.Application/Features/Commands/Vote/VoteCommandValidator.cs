using FluentValidation;

namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteCommandValidator : AbstractValidator<VoteCommand>
{
    private static readonly HashSet<string> AllowedValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "0", "0.5", "1", "2", "3", "5", "8", "13", "21", "?"
    };

    public VoteCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0);

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .NotEmpty()
            .Must(v => AllowedValues.Contains(v))
                .WithMessage($"Value must be one of: {string.Join(", ", AllowedValues)}");
    }
}
