using FluentValidation;

namespace ScrumPoker.API.Features.JoinRoom;

public sealed class JoinRoomRequestValidator : AbstractValidator<JoinRoomRequest>
{
    public JoinRoomRequestValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);
    }
}
