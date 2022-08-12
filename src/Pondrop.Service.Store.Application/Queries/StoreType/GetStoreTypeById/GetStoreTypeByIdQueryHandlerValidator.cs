using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreTypeByIdQueryHandlerValidator : AbstractValidator<GetStoreTypeByIdQuery>
{
    public GetStoreTypeByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}