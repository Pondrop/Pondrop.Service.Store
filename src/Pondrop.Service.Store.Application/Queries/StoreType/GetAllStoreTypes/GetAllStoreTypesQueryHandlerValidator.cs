using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoreTypesQueryHandlerValidator : AbstractValidator<GetAllStoreTypesQuery>
{
    public GetAllStoreTypesQueryHandlerValidator()
    {
    }
}