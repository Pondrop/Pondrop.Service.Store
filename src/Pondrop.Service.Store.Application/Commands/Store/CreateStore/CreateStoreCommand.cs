using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateStoreCommand : IRequest<Result<StoreRecord>>
{
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ExternalReferenceId { get; init; } = string.Empty;

    public AddressRecord? Address { get; init; } = default;

    public Guid RetailerId { get; init; } = Guid.Empty;
    public Guid StoreTypeId { get; init; } = Guid.Empty;
}

public record AddressRecord(
    string ExternalReferenceId,
    string AddressLine1,
    string? AddressLine2,
    string Suburb,
    string State,
    string Postcode,
    string Country,
    double Latitude,
    double Longitude)
{
    public AddressRecord() : this(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0
    )
    {
    }
}
    