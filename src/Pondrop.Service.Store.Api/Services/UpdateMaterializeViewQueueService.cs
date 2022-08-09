using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public class UpdateMaterializeViewQueueService : BaseBackgroundQueueService<UpdateMaterializedViewByIdCommand>, IUpdateMaterializeViewQueueService
{
}