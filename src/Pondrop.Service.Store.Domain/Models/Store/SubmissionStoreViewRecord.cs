using System.Spatial;

namespace Pondrop.Service.Store.Domain.Models;

public record SubmissionStoreViewRecord(
        Guid StoreId,
        string Name,
        string RetailerName)
{
    public SubmissionStoreViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
    {
    }
}