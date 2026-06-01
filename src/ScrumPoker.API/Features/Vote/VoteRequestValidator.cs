using FluentValidation;

namespace ScrumPoker.API.Features.Vote;

public sealed class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .NotEmpty();
    }
}
