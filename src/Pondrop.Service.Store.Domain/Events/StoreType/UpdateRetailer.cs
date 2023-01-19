using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.StoreType;

public record UpdateStoreType(string Name, string Sector) : EventPayload;
