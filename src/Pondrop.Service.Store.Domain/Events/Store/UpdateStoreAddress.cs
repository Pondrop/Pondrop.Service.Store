using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.Store;

public record UpdateStoreAddress(
    Guid Id,
    Guid StoreId,
    string? AddressLine1,
    string? AddressLine2,
    string? Suburb,
    string? State,
    string? Postcode,
    string? Country,
    double? Latitude,
    double? Longitude) : EventPayload;
