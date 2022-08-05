using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record UpdateStore(
    string? Name,
    string? Status,
    RetailerRecord? Retailer,
    StoreTypeRecord? StoreType) : EventPayload;
    