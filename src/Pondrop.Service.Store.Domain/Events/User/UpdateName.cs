namespace Pondrop.Service.Store.Domain.Events.User;

public record UpdateName(string FirstName, string LastName) : EventPayload;