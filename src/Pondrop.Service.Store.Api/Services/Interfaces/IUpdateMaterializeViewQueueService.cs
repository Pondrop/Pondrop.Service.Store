using Pondrop.Service.Store.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.Store.Api.Services;

public interface IUpdateMaterializeViewQueueService
{
    Task<UpdateMaterializedViewByIdCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(UpdateMaterializedViewByIdCommand command);
}