namespace Pondrop.Service.Store.Api.Services.Interface;

public interface IServiceBusListener
{
    Task HandleMessages();

    Task CloseListener();

    ValueTask DisposeAsync();
}
