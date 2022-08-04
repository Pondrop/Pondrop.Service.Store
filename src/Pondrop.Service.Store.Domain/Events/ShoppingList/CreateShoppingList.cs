namespace Pondrop.Service.Store.Domain.Events.ShoppingList;

public record CreateShoppingList(Guid Id, Guid UserId, string Name, string Description) : EventPayload;