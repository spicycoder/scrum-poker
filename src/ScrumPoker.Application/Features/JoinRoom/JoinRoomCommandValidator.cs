using FluentValidation;

namespace ScrumPoker.Application.Features.JoinRoom;

public sealed class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
{
    public JoinRoomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0);

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);
    }
}
