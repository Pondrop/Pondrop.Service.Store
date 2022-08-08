using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class UpdateMaterializedViewByIdCommand<T> : IRequest<T>
{
    public Guid Id { get; init; } = Guid.Empty;
}