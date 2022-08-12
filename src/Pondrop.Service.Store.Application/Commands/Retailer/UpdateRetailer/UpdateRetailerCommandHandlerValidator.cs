using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerCommandHandlerValidator : AbstractValidator<UpdateRetailerCommand>
{
    public UpdateRetailerCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}