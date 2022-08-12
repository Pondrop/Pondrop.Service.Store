using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreAddressCommand : IRequest<Result<StoreRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public Guid StoreId { get; init; } = Guid.Empty;
    public string? AddressLine1 { get; init; } = null;
    public string? AddressLine2 { get; init; } = null;
    public string? Suburb { get; init; } = null;
    public string? State { get; init; } = null;
    public string? Postcode { get; init; } = null;
    public string? Country { get; init; } = null;
    public double? Latitude { get; init; } = null;
    public double? Longitude { get; init; } = null;
}