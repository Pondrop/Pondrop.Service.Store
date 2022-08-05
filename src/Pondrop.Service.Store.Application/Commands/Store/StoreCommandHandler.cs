using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class StoreCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly StoreUpdateConfiguration _storeUpdateConfig;
    private readonly IDaprService _daprService; 
    private readonly ILogger _logger;

    public StoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _storeUpdateConfig = storeUpdateConfig.Value;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task InvokeDaprMethods(Guid storeId, IEnumerable<IEvent> events)
    {
        if (storeId != Guid.Empty && events.Any())
        {
            // Update Materialized View
            if (!string.IsNullOrWhiteSpace(_storeUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_storeUpdateConfig.MethodName))
            {
                var viewUpdated = await _daprService.InvokeServiceAsync(
                    _storeUpdateConfig.AppId,
                    _storeUpdateConfig.MethodName,
                    new UpdateStoreMaterializedViewByIdCommand() { Id = storeId });
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            }

            // Send Events to Event Grid
            if (!string.IsNullOrWhiteSpace(_storeUpdateConfig.EventTopic))
            {
                var bindingInvoked = await _daprService.SendEventsAsync(_storeUpdateConfig.EventTopic, events);
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            }
        }
    }
}