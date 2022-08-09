using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreTypeMaterializedViewCommandHandler : RebuildMaterializedViewCommandHandler<RebuildStoreTypeMaterializedViewCommand, StoreTypeEntity>
{
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly ILogger<RebuildStoreTypeMaterializedViewCommandHandler> _logger;

    public RebuildStoreTypeMaterializedViewCommandHandler(
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        ILogger<RebuildStoreTypeMaterializedViewCommandHandler> logger) : base(storeTypeViewRepository, logger)
    {
        _storeTypeViewRepository = storeTypeViewRepository;
        _logger = logger;
    }
}