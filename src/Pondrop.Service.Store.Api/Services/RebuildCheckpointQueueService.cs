using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public class RebuildCheckpointQueueService : BaseBackgroundQueueService<RebuildCheckpointCommand>, IRebuildCheckpointQueueService
{
}