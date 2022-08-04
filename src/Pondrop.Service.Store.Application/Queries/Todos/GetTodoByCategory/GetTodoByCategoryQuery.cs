using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByCategoryQuery : IRequest<Result<List<TodoItem>>>
{
    public string Category { get; init; } = string.Empty;
}