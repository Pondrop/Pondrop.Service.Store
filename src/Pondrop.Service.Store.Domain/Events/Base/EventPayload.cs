using Newtonsoft.Json;

namespace Pondrop.Service.Store.Domain.Events;

public record EventPayload : IEventPayload
{
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;
}