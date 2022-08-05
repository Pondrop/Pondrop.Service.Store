using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RemoveAddressFromStoreCommand : IRequest<Result<StoreRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public Guid StoreId { get; init; } = Guid.Empty;
}