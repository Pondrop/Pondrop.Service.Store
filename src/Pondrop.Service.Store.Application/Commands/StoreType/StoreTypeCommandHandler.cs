using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class StoreTypeCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly StoreTypeUpdateConfiguration _storeTypeUpdateConfig;
    private readonly IDaprService _daprService; 
    private readonly ILogger _logger;

    public StoreTypeCommandHandler(
        IOptions<StoreTypeUpdateConfiguration> storeTypeUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _storeTypeUpdateConfig = storeTypeUpdateConfig.Value;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task InvokeDaprMethods(Guid storeTypeId, IEnumerable<IEvent> events)
    {
        if (storeTypeId != Guid.Empty && events.Any())
        {
            // Update Materialized View
            if (!string.IsNullOrWhiteSpace(_storeTypeUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_storeTypeUpdateConfig.MethodName))
            {
                var viewUpdated = await _daprService.InvokeServiceAsync(
                    _storeTypeUpdateConfig.AppId,
                    _storeTypeUpdateConfig.MethodName,
                    new UpdateStoreTypeMaterializedViewByIdCommand() { Id = storeTypeId });
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            }

            // Send Events to Event Grid
            if (!string.IsNullOrWhiteSpace(_storeTypeUpdateConfig.EventTopic))
            {
                var bindingInvoked = await _daprService.SendEventsAsync(_storeTypeUpdateConfig.EventTopic, events);
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            }
        }
    }
}