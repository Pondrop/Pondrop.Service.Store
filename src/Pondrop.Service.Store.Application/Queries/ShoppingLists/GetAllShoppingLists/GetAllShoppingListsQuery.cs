using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllShoppingListsQuery : IRequest<Result<List<ShoppingListRecord>>>
{
    public string StreamType { get; } = EventEntity.GetStreamTypeName<ShoppingListEntity>();
}