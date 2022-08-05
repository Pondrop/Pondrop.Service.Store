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
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc);