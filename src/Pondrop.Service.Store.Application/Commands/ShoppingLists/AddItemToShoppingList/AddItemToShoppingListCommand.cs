using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class AddItemToShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid ShoppingListId { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
}