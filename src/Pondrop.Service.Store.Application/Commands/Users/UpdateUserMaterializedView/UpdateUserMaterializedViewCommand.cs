using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserMaterializedViewCommand : IRequest<Result<UserEntity>>
{
    public Guid Id { get; init; } = Guid.Empty;
}