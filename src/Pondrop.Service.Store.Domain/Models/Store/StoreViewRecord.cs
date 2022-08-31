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
        List<StoreAddressRecord> Addresses,
        Guid RetailerId,
        RetailerRecord Retailer,
        Guid StoreTypeId,
        StoreTypeRecord StoreType,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : StoreRecord(Id, Name, Status, ExternalReferenceId, Phone, Email, OpenHours, Addresses, RetailerId, StoreTypeId, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StoreViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<StoreAddressRecord>(0),
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