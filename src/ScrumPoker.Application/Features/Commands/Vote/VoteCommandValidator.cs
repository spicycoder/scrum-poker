using FluentValidation;

namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteCommandValidator : AbstractValidator<VoteCommand>
{
    public VoteCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0);

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .NotEmpty();
    }
}
