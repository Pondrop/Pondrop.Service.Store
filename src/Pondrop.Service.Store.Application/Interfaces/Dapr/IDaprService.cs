using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Interfaces;

public interface IDaprService
{
    Task<bool> InvokeServiceAsync(string appId, string methodName, object? data = null);

    Task<bool> SendEventsAsync(string eventGridTopic, IEnumerable<IEvent> events);
}