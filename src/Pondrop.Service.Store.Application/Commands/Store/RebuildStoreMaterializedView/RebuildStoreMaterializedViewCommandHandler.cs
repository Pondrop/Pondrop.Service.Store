using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreMaterializedViewCommandHandler : RebuildMaterializedViewCommandHandler<RebuildStoreMaterializedViewCommand, StoreEntity>
{
    public RebuildStoreMaterializedViewCommandHandler(
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        ILogger<RebuildStoreMaterializedViewCommandHandler> logger) : base(storeViewRepository, logger)
    {
    }
}