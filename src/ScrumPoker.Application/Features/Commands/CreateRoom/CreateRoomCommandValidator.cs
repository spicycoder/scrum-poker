using FluentValidation;

namespace ScrumPoker.Application.Features.Commands.CreateRoom;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .MaximumLength(50);
    }
}
