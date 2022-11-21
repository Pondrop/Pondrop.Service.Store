using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.Retailer;

public record UpdateRetailer(string Name) : EventPayload;
