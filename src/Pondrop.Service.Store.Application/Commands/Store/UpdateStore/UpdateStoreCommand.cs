using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreCommand : IRequest<Result<StoreRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string? Name { get; init; } = null;
    public string? Status { get; init; } = null;
    public string? Phone { get; init; } = null;
    public string? Email { get; init; } = null;
    public string? OpenHours { get; init; } = null;

    public Guid? RetailerId { get; init; } = null;
    public Guid? StoreTypeId { get; init; } = null;
}