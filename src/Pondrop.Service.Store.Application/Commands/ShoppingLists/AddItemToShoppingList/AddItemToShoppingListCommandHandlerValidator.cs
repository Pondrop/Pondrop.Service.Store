using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class AddItemToShoppingListCommandHandlerValidator : AbstractValidator<AddItemToShoppingListCommand>
{
    public AddItemToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}