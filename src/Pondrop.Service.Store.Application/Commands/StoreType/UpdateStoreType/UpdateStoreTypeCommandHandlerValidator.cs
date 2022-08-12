using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeCommandHandlerValidator : AbstractValidator<UpdateStoreTypeCommand>
{
    public UpdateStoreTypeCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}