using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserEmailCommandHandlerValidator : AbstractValidator<UpdateUserEmailCommand>
{
    public UpdateUserEmailCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}