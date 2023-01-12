using Pondrop.Service.Events;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record UpdateStore(
    string? Name,
    string? Status,
    string? Phone,
    string? Email,
    string? OpenHours,
    bool IsCommunityStore,
    Guid? RetailerId,
    Guid? StoreTypeId) : EventPayload;
