using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record CreateStore(
    Guid Id, 
    string Name,
    string Status,
    string ExternalReferenceId,
    RetailerRecord Retailer,
    StoreTypeRecord StoreType) : EventPayload;
    