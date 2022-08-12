using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreByIdQueryHandlerValidator : AbstractValidator<GetStoreByIdQuery>
{
    public GetStoreByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}