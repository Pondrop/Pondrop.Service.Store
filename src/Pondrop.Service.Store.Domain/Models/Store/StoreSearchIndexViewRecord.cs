using Microsoft.Azure.Cosmos.Spatial;
using System.Spatial;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreSearchIndexViewRecord(
        Guid Id,
        string Name,
        string Status,
        string ExternalReferenceId,
        string Phone,
        string Email,
        string OpenHours,
        Guid AddressId,
        string AddressExternalReferenceId,
        string AddressLine1,
        string AddressLine2,
        string Suburb,
        string State,
        string Postcode,
        string Country,
        double Latitude,
        double Longitude,
        Point LocationSort,
        Guid RetailerId,
        RetailerRecord Retailer,
        Guid StoreTypeId,
        StoreTypeRecord StoreType,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
{
    public StoreSearchIndexViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        new Point(0, 0),
        Guid.Empty,
        new RetailerRecord(),
        Guid.Empty,
        new StoreTypeRecord(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}