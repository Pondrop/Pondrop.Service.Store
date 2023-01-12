using Pondrop.Service.Events;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record CreateStore(
    Guid Id,
    string Name,
    string Status,
    string Phone,
    string Email,
    string OpenHours,
    string ExternalReferenceId,
    bool IsCommunityStore,
    Guid RetailerId,
    Guid StoreTypeId) : EventPayload;
