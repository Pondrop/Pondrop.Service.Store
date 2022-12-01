using System.Spatial;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreViewRecord(
        Guid Id,
        string Name,
        string Status,
        string ExternalReferenceId,
        string Phone,
        string Email,
        string OpenHours,
        Guid RetailerId,
        RetailerRecord Retailer,
        Guid StoreTypeId,
        StoreTypeRecord StoreType,
        List<StoreAddressRecord> Addresses,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
{
    public StoreViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        Guid.Empty,
        new RetailerRecord(),
        Guid.Empty,
        new StoreTypeRecord(),
        new List<StoreAddressRecord>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}