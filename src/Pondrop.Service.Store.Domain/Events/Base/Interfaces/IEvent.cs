using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Store.Domain.Events;

public interface IEvent
{
    string StreamId { get; }
    string StreamType { get; }
    long SequenceNumber { get; }
    string EventPayloadType { get; }
    JObject Payload { get; }
    string CreatedBy { get; }
    DateTime CreatedUtc { get; }

    IEventPayload? GetEventPayload();
    IEventPayload? GetEventPayload(IEventTypePayloadResolver eventTypePayloadResolver);
}