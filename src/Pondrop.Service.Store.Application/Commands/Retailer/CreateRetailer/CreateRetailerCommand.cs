using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateRetailerCommand : IRequest<Result<RetailerRecord>>
{
    public string Name { get; init; } = string.Empty;
    public string ExternalReferenceId { get; init; } = string.Empty;
}