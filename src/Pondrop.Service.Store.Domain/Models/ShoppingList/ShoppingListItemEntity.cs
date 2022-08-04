using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.ShoppingList;
using Pondrop.Service.Store.Domain.Events.User;

namespace Pondrop.Service.Store.Domain.Models;

public record ShoppingListItemEntity(Guid Id, Guid ShoppingListId, string Name);