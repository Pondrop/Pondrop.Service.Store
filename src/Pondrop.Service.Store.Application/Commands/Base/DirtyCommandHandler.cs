using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class DirtyCommandHandler<TRequest, TResponse, TEntity, TUpdateByIdCommand> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TEntity : EventEntity
    where TUpdateByIdCommand : UpdateMaterializedViewByIdCommand<TResponse>, new()
{
    private readonly IMaterializedViewRepository<TEntity> _materializedViewRepository;
    private readonly DaprEventTopicConfiguration _daprUpdateConfig;
    private readonly IDaprService _daprService; 
    private readonly ILogger _logger;

    public DirtyCommandHandler(
        IMaterializedViewRepository<TEntity> materializedViewRepository,
        DaprEventTopicConfiguration daprUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _materializedViewRepository = materializedViewRepository;
        _daprUpdateConfig = daprUpdateConfig;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task<TEntity?> UpdateMaterializedView(long expectedVersion, TEntity entity)
    {
        if (expectedVersion >= 0)
        {
            // Update Materialized View
            return await _materializedViewRepository.UpsertAsync(expectedVersion, entity);
        }

        return null;
    }
    
    protected async Task InvokeDaprMethods(Guid id, IEnumerable<IEvent> events)
    {
        if (id != Guid.Empty && events.Any())
        {
            // Update Materialized View
            // if (!string.IsNullOrWhiteSpace(_daprUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_daprUpdateConfig.MethodName))
            // {
            //     var viewUpdated = await _daprService.InvokeServiceAsync(
            //         _daprUpdateConfig.AppId,
            //         _daprUpdateConfig.MethodName,
            //         new TUpdateByIdCommand() { Id = id });
            //     System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            // }

            // Send Events to Event Grid
            if (!string.IsNullOrWhiteSpace(_daprUpdateConfig.EventTopic))
            {
                var bindingInvoked = await _daprService.SendEventsAsync(_daprUpdateConfig.EventTopic, events);
                System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            }
        }
    }
}