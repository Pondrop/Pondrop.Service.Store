namespace Pondrop.Service.Store.Domain.Events.ShoppingList;

public record RemoveItemFromShoppingList(Guid Id, Guid ShoppingListId) : EventPayload;