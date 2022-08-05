namespace Pondrop.Service.Store.Domain.Events.Store;

public record AddStoreAddress(
    Guid Id,
    Guid StoreId,
    string ExternalReferenceId,
    string AddressLine1,
    string AddressLine2,
    string Suburb,
    string State,
    string Postcode,
    string Country,
    double Latitude,
    double Longitude) : EventPayload;
    