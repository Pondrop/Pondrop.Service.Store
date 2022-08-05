using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class RetailerCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly RetailerUpdateConfiguration _retailerUpdateConfig;
    private readonly IDaprService _daprService; 
    private readonly ILogger _logger;

    public RetailerCommandHandler(
        IOptions<RetailerUpdateConfiguration> retailerUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _retailerUpdateConfig = retailerUpdateConfig.Value;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task InvokeDaprMethods(Guid retailerId, IEnumerable<IEvent> events)
    {
        if (retailerId != Guid.Empty && events.Any())
        {
            // Update Materialized View
            if (!string.IsNullOrWhiteSpace(_retailerUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_retailerUpdateConfig.MethodName))
            {
                var viewUpdated = await _daprService.InvokeServiceAsync(
                    _retailerUpdateConfig.AppId,
                    _retailerUpdateConfig.MethodName,
                    new UpdateRetailerMaterializedViewByIdCommand() { Id = retailerId });
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            }

            // Send Events to Event Grid
            if (!string.IsNullOrWhiteSpace(_retailerUpdateConfig.EventTopic))
            {
                var bindingInvoked = await _daprService.SendEventsAsync(_retailerUpdateConfig.EventTopic, events);
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            }
        }
    }
}