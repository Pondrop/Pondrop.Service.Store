using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersAndShoppingListsQueryHandlerValidator : AbstractValidator<GetAllUsersWithShoppingListsQuery>
{
    public GetAllUsersAndShoppingListsQueryHandlerValidator()
    {
        RuleFor(x => x.UserStreamType).NotEmpty();
        RuleFor(x => x.ShoppingListStreamType).NotEmpty();
    }
}