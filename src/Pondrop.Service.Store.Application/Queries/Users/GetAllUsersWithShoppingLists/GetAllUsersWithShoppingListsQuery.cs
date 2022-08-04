using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersWithShoppingListsQuery : IRequest<Result<List<UserWithShoppingListsRecord>>>
{
    public string UserStreamType { get; } = EventEntity.GetStreamTypeName<UserEntity>();
    public string ShoppingListStreamType { get; } = EventEntity.GetStreamTypeName<ShoppingListEntity>();
}