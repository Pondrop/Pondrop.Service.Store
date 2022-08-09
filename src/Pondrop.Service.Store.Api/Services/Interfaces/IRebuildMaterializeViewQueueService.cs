using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public interface IRebuildMaterializeViewQueueService
{
    Task<RebuildMaterializedViewCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildMaterializedViewCommand command);
}