using Pondrop.Service.Events;

namespace Pondrop.Service.Store.Domain.Events.Retailer;

public record CreateRetailer(Guid Id, string ExternalReferenceId, string Name) : EventPayload;
