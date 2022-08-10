using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildStoreCheckpointCommand, StoreEntity>
{
    public RebuildStoreCheckpointCommandHandler(
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        ILogger<RebuildStoreCheckpointCommandHandler> logger) : base(storeCheckpointRepository, logger)
    {
    }
}