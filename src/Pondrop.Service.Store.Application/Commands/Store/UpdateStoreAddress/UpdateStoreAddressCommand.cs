using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreAddressCommand : IRequest<Result<StoreRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public Guid StoreId { get; init; } = Guid.Empty;
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? Suburb { get; init; }
    public string? State { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}