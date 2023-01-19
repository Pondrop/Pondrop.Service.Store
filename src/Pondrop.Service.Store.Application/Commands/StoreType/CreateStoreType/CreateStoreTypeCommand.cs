using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateStoreTypeCommand : IRequest<Result<StoreTypeRecord>>
{
    public string Name { get; init; } = string.Empty;
    public string ExternalReferenceId { get; init; } = string.Empty;
    public string Sector { get; init; } = string.Empty;
}