namespace Pondrop.Service.Store.Domain.Models;

public record AuditRecord(string CreatedBy, string UpdatedBy, DateTime CreatedUtc, DateTime UpdatedUtc);