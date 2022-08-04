using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class UserCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly UserUpdateConfiguration _userUpdateConfig;
    private readonly IDaprService _daprService; 
    private readonly ILogger _logger;

    public UserCommandHandler(
        IOptions<UserUpdateConfiguration> userUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _userUpdateConfig = userUpdateConfig.Value;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task InvokeDaprMethods(Guid userId, IEnumerable<IEvent> events)
    {
        if (userId != Guid.Empty && events.Any())
        {
            // Update Materialized View
            if (!string.IsNullOrWhiteSpace(_userUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_userUpdateConfig.MethodName))
            {
                var viewUpdated = await _daprService.InvokeServiceAsync(
                    _userUpdateConfig.AppId,
                    _userUpdateConfig.MethodName,
                    new UpdateUserMaterializedViewCommand() { Id = userId });
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            }

            // Send Events to Event Grid
            if (!string.IsNullOrWhiteSpace(_userUpdateConfig.EventTopic))
            {
                var bindingInvoked = await _daprService.SendEventsAsync(_userUpdateConfig.EventTopic, events);
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            }
        }
    }
}