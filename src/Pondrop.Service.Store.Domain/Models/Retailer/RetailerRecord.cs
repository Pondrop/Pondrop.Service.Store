namespace Pondrop.Service.Store.Domain.Models;

public record RetailerRecord(
        Guid Id,
        string ExternalReferenceId,
        string Name,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public RetailerRecord() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue)
    {
    }
}