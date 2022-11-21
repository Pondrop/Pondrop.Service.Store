using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.StoreType;

public record CreateStoreType(Guid Id, string ExternalReferenceId, string Name) : EventPayload;
