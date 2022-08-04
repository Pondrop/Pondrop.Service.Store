using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByIdQuery : IRequest<Result<TodoItem>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Category { get; init; } = string.Empty;
}