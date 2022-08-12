using FluentValidation;
using Pondrop.Service.Store.Application.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreCommandHandlerValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}