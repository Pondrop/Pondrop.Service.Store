using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerMaterializedViewByIdValidator : AbstractValidator<UpdateRetailerMaterializedViewByIdCommand>
{
    public UpdateRetailerMaterializedViewByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}