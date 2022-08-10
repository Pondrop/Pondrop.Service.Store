using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreTypeCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildStoreTypeCheckpointCommand, StoreTypeEntity>
{
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly ILogger<RebuildStoreTypeCheckpointCommandHandler> _logger;

    public RebuildStoreTypeCheckpointCommandHandler(
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        ILogger<RebuildStoreTypeCheckpointCommandHandler> logger) : base(storeTypeCheckpointRepository, logger)
    {
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
        _logger = logger;
    }
}