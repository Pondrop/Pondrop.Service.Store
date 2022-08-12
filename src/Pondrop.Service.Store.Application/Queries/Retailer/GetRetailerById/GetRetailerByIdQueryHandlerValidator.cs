using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetRetailerByIdQueryHandlerValidator : AbstractValidator<GetRetailerByIdQuery>
{
    public GetRetailerByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}