using Pondrop.Service.Store.Api.Services.Interface;

namespace Pondrop.Service.Store.Api.Worker;

public class WorkerServiceBus : IHostedService, IDisposable
{
    private readonly ILogger<WorkerServiceBus> _logger;
    private readonly IServiceBusListener _serviceBusTopicSubscription;

    public WorkerServiceBus(IServiceBusListener serviceBusTopicSubscription,
        ILogger<WorkerServiceBus> logger)
    {
        _serviceBusTopicSubscription = serviceBusTopicSubscription;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Starting the service bus queue consumer and the subscription");
        await _serviceBusTopicSubscription.PrepareFiltersAndHandleMessages().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Stopping the service bus queue consumer and the subscription");
        await _serviceBusTopicSubscription.CloseSubscriptionAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async void Dispose(bool disposing)
    {
        if (disposing)
        {
            await _serviceBusTopicSubscription.DisposeAsync().ConfigureAwait(false);
        }
    }
}