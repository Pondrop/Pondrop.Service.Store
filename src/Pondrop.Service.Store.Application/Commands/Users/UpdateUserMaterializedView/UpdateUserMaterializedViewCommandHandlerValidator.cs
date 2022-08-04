using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserMaterializedViewCommandHandlerValidator : AbstractValidator<UpdateUserMaterializedViewCommand>
{
    public UpdateUserMaterializedViewCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}