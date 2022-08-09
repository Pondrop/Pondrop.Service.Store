using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildRetailerMaterializedViewCommandHandler : RebuildMaterializedViewCommandHandler<RebuildRetailerMaterializedViewCommand, RetailerEntity>
{
    public RebuildRetailerMaterializedViewCommandHandler(
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        ILogger<RebuildRetailerMaterializedViewCommandHandler> logger) : base(retailerViewRepository, logger)
    {
    }
}