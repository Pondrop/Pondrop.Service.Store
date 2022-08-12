using Pondrop.Service.Store.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.Store.Api.Services;

public interface IServiceBusListenerService
{
    Task StartListener();

    Task StopListener();

    ValueTask DisposeAsync();
}