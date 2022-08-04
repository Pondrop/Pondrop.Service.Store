using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetUserByIdQueryHandlerValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}