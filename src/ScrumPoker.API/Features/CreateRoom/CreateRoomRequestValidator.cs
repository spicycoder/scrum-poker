using FluentValidation;

namespace ScrumPoker.API.Features.CreateRoom;

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CardSet)
            .NotEmpty();
    }
}
