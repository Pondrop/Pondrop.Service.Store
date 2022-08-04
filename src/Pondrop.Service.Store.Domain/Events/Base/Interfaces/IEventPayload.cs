using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Store.Domain.Events;

public interface IEventPayload
{
    DateTime CreatedUtc { get; }
}