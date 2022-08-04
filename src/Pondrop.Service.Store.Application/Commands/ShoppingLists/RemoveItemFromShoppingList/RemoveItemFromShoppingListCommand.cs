using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RemoveItemFromShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}