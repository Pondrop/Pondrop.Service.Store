using FluentValidation;
using Pondrop.Service.Store.Domain.Events.Retailer;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateRetailerCommandHandlerValidator : AbstractValidator<CreateRetailerCommand>
{
    public CreateRetailerCommandHandlerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.ExternalReferenceId).NotEmpty();
    }
}