using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public interface IRebuildCheckpointQueueService
{
    Task<RebuildCheckpointCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildCheckpointCommand command);
}