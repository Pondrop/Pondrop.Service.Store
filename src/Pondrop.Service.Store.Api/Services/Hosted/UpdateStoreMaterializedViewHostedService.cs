using Pondrop.Service.Store.Api.Services.Interface;

namespace Pondrop.Service.Store.Api.Worker;

public class WorkerServiceBus : IHostedService, IDisposable
{
    private readonly ILogger<WorkerServiceBus> _logger;
    private readonly IServiceBusListener _serviceBusListener;

    public WorkerServiceBus(IServiceBusListener serviceBusTopicSubscription,
        ILogger<WorkerServiceBus> logger)
    {
        _serviceBusListener = serviceBusTopicSubscription;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        await _serviceBusListener.HandleMessages().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        await _serviceBusListener.CloseListener().ConfigureAwait(false);
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
            await _serviceBusListener.DisposeAsync().ConfigureAwait(false);
        }
    }
}