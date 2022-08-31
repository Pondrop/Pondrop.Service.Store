namespace Pondrop.Service.Store.Domain.Models;

public record StoreRecord(
        Guid Id,
        string Name,
        string Status,
        string ExternalReferenceId,
        string Phone,
        string Email,
        string OpenHours,
        List<StoreAddressRecord> Addresses,
        Guid RetailerId,
        Guid StoreTypeId,
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
        string.Empty,
        string.Empty,
        string.Empty,
        new List<StoreAddressRecord>(0),
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}