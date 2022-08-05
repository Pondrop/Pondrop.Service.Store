using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoresQueryHandlerValidator : AbstractValidator<GetAllStoresQuery>
{
    public GetAllStoresQueryHandlerValidator()
    {
    }
}