using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByCategoryQueryHandlerValidator : AbstractValidator<GetTodoByCategoryQuery>
{
    public GetTodoByCategoryQueryHandlerValidator()
    {
        RuleFor(x => x.Category).NotEmpty();
    }
}