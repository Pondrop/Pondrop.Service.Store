using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Store.Application.Interfaces.ServiceBus;
public interface IServiceBusMessagingService<T> where T : new()
{
    Task SendMessageAsync(T message);
}

