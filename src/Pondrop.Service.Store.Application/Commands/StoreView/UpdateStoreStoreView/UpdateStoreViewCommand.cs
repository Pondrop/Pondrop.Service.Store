using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreViewCommand : IRequest<Result<List<SubmissionStoreViewRecord>>>
{
    public Guid? StoreId { get; init; } = null;
    public Guid? RetailerId { get; init; } = null;
    public Guid? StoreTypeId { get; init; } = null;
}