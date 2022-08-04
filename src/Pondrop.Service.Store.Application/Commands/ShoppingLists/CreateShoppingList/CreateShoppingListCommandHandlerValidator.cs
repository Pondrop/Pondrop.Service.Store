using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateShoppingListCommandHandlerValidator : AbstractValidator<CreateShoppingListCommand>
{
    public CreateShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotNull();
    }
}