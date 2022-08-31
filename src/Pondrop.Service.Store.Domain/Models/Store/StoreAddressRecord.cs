using Microsoft.Azure.Cosmos.Spatial;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreAddressRecord(
        Guid Id,
        string ExternalReferenceId,
        string AddressLine1,
        string AddressLine2,
        string Suburb,
        string State,
        string Postcode,
        string Country,
        double Latitude,
        double Longitude,
        Point LocationSort,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StoreAddressRecord() : this(
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
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}
