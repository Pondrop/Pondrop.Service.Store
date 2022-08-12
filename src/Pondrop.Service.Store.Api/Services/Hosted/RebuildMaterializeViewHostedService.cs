using MediatR;
using Pondrop.Service.Store.Application.Commands;

namespace Pondrop.Service.Store.Api.Services;

public class RebuildMaterializeViewHostedService : BackgroundService
{
    private readonly IRebuildCheckpointQueueService _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RebuildMaterializeViewHostedService> _logger;

    public RebuildMaterializeViewHostedService(
        IRebuildCheckpointQueueService queue,
        IServiceProvider serviceProvider,
        ILogger<RebuildMaterializeViewHostedService> logger)
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

            try
            {
                var mediator = _serviceProvider.GetService<IMediator>();
                await mediator!.Send(command, stoppingToken);

                switch (command)
                {
                    case RebuildStoreCheckpointCommand store:
                        await mediator!.Send(new RebuildStoreViewCommand(), stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
            }
        }
    }
}