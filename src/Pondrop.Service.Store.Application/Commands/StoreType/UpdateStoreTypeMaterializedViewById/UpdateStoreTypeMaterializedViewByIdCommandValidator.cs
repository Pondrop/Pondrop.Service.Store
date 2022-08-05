using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeMaterializedViewCommandValidator : AbstractValidator<UpdateStoreTypeMaterializedViewByIdCommand>
{
    public UpdateStoreTypeMaterializedViewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}