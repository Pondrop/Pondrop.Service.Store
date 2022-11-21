using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record RemoveAddressFromStore(
    Guid Id,
    Guid StoreId) : EventPayload;
