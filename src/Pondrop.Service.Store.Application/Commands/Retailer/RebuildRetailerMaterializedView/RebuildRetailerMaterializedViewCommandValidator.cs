using FluentValidation;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildRetailerMaterializedViewCommandValidator : AbstractValidator<RebuildRetailerMaterializedViewCommand>
{
    public RebuildRetailerMaterializedViewCommandValidator()
    {
    }
}