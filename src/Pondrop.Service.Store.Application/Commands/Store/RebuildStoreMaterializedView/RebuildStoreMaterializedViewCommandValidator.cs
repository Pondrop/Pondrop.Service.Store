using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreMaterializedViewCommandValidator : AbstractValidator<RebuildStoreMaterializedViewCommand>
{
    public RebuildStoreMaterializedViewCommandValidator()
    {
    }
}