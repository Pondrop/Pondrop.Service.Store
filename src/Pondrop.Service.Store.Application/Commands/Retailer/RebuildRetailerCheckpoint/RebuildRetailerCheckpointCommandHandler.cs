using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildRetailerCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildRetailerCheckpointCommand, RetailerEntity>
{
    public RebuildRetailerCheckpointCommandHandler(
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        ILogger<RebuildRetailerCheckpointCommandHandler> logger) : base(retailerCheckpointRepository, logger)
    {
    }
}