using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateMaterializedViewByIdCommandValidator : AbstractValidator<UpdateMaterializedViewByIdCommand>
{
    public UpdateMaterializedViewByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}