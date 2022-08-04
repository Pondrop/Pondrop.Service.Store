using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByIdQueryHandlerValidator : AbstractValidator<GetTodoByIdQuery>
{
    public GetTodoByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
    }
}