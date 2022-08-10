using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Application.Interfaces;

public interface IServiceBusService
{
    Task SendMessageAsync(object payload);

    Task SendMessageAsync(string subject, object payload);
}