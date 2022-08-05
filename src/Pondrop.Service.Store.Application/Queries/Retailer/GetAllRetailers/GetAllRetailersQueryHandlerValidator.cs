using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllRetailersQueryHandlerValidator : AbstractValidator<GetAllRetailersQuery>
{
    public GetAllRetailersQueryHandlerValidator()
    {
    }
}