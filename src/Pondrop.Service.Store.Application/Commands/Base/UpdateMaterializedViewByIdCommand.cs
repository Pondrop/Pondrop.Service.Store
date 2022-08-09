using MediatR;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class UpdateMaterializedViewByIdCommand
{
    public Guid Id { get; init; } = Guid.Empty;
}

public abstract class UpdateMaterializedViewByIdCommand<T> : UpdateMaterializedViewByIdCommand, IRequest<T> 
{
}