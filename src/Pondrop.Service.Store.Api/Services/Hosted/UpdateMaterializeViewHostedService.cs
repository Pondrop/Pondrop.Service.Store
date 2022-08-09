using MediatR;
using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public class UpdateMaterializeViewHostedService : BackgroundService
{
    private readonly IUpdateMaterializeViewQueueService _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateMaterializeViewHostedService> _logger;

    public UpdateMaterializeViewHostedService(
        IUpdateMaterializeViewQueueService queue,
        IServiceProvider serviceProvider,
        ILogger<UpdateMaterializeViewHostedService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var command = await _queue.DequeueAsync(stoppingToken);
            
            if (command.Id == Guid.Empty)
                continue;

            try
            {
                using var scoped = _serviceProvider.CreateScope();
                var mediator = scoped.ServiceProvider.GetService<IMediator>();
                await mediator!.Send(command, stoppingToken);

                switch (command)
                {
                    case UpdateRetailerMaterializedViewByIdCommand retailer:
                        await mediator!.Send(new UpdateStoreRelationshipsCommand() { RetailerId = retailer.Id}, stoppingToken);
                        break;
                    case UpdateStoreTypeMaterializedViewByIdCommand storeType:
                        await mediator!.Send(new UpdateStoreRelationshipsCommand() { StoreTypeId = storeType.Id}, stoppingToken);
                        break;;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
            }
        }
    }
}