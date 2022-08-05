using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreTypeMaterializedViewCommandValidator : AbstractValidator<RebuildStoreTypeMaterializedViewCommand>
{
    public RebuildStoreTypeMaterializedViewCommandValidator()
    {
    }
}