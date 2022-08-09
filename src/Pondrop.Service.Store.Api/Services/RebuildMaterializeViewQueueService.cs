using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public class RebuildMaterializeViewQueueService : BaseBackgroundQueueService<RebuildMaterializedViewCommand>, IRebuildMaterializeViewQueueService
{
}