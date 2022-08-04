using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllShoppingListsQueryHandlerValidator : AbstractValidator<GetAllShoppingListsQuery>
{
    public GetAllShoppingListsQueryHandlerValidator()
    {
        RuleFor(x => x.StreamType).NotEmpty();
    }
}