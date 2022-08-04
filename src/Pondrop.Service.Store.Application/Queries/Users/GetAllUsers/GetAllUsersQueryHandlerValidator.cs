using FluentValidation;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersQueryHandlerValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryHandlerValidator()
    {
        RuleFor(x => x.StreamType).NotEmpty();
    }
}