using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreMaterializedViewByIdCommandValidator : AbstractValidator<UpdateStoreMaterializedViewByIdCommand>
{
    public UpdateStoreMaterializedViewByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}