using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserEmailCommand : IRequest<Result<UserRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Email { get; init; } = string.Empty;
}