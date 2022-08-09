using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreRelationshipsCommand : IRequest<Result<int>>
{
    public Guid RetailerId { get; init; } = Guid.Empty;
    public Guid StoreTypeId { get; init; } = Guid.Empty;
}