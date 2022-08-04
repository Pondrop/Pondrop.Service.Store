namespace Pondrop.Service.Store.Domain.Models;

public record UserWithShoppingListsRecord(Guid Id, string FirstName, string LastName, string Email, List<ShoppingListRecord> ShoppingLists)
    : UserRecord(Id, FirstName, LastName, Email);