using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateStoreTypeCommandHandlerValidator : AbstractValidator<CreateStoreTypeCommand>
{
    public CreateStoreTypeCommandHandlerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.ExternalReferenceId).NotEmpty();
    }
}