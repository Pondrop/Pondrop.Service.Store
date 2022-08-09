namespace Pondrop.Service.Store.Api.Services.Interface;

public interface IServiceBusListener
{
    Task PrepareFiltersAndHandleMessages();

    Task CloseSubscriptionAsync();

    ValueTask DisposeAsync();
}
