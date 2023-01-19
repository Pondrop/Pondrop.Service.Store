using Pondrop.Service.Models;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreTypeRecord(
        Guid Id,
        string ExternalReferenceId,
        string Name,
        string Sector,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StoreTypeRecord() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue)
    {
    }
}