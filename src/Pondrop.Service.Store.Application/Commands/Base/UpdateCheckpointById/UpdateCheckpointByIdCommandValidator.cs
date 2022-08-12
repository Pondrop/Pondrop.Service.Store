using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateCheckpointByIdCommandValidator : AbstractValidator<UpdateCheckpointByIdCommand>
{
    public UpdateCheckpointByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}