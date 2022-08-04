using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersQuery : IRequest<Result<List<UserRecord>>>
{
    public string StreamType { get; } = EventEntity.GetStreamTypeName<UserEntity>();
}