namespace Pondrop.Service.Store.Domain.Models;

public record StoreRecord(
        Guid Id,
        string Name,
        string Status,
        string ExternalReferenceId,
        List<StoreAddressRecord> Addresses,
        RetailerRecord Retailer,
        StoreTypeRecord StoreType,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StoreRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<StoreAddressRecord>(0),
        new RetailerRecord(),
        new StoreTypeRecord(),
        string.Empty, 
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}