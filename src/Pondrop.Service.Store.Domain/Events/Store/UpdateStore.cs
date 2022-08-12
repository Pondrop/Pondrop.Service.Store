using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record UpdateStore(
    string? Name,
    string? Status,
    Guid? RetailerId,
    Guid? StoreTypeId) : EventPayload;
