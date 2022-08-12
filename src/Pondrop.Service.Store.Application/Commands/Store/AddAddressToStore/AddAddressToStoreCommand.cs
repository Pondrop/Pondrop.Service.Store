using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class AddAddressToStoreCommand : IRequest<Result<StoreRecord>>
{
    public Guid StoreId { get; init; } = Guid.Empty;
    public string ExternalReferenceId { get; init; } = string.Empty;
    public string AddressLine1 { get; init; } = string.Empty;
    public string AddressLine2 { get; init; } = string.Empty;
    public string Suburb { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Postcode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}