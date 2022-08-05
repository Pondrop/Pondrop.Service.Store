using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Store.Domain.Events;

public class Event : IEvent
{
    public Event()
    {
        StreamId = string.Empty;
        StreamType = string.Empty;
        SequenceNumber = -1;
        EventPayloadType = string.Empty;
        Payload = new JObject();
        CreatedBy = string.Empty;
        CreatedUtc = DateTime.MinValue;
    }

    public Event(string streamId, string streamType, long sequenceNumber, IEventPayload payload, string createdBy)
    {
        StreamId = streamId;
        StreamType = streamType;
        SequenceNumber = sequenceNumber;
        EventPayloadType = payload.GetType().Name;
        Payload = JObject.FromObject(payload);
        CreatedBy = createdBy;
        CreatedUtc = DateTime.UtcNow;
    }
    
    [JsonProperty("streamId")]
    public string StreamId { get; init; }
    
    [JsonProperty("streamType")]
    public string StreamType { get; init; }
    
    [JsonProperty("sequenceNumber")]
    public long SequenceNumber { get; init; }

    [JsonProperty("eventPayloadType")]
    public string EventPayloadType { get; init; }
    
    [JsonProperty("payload")]
    public JObject Payload { get; init; }
    
    [JsonProperty("createdBy")]
    public string CreatedBy { get; init; }
    
    [JsonProperty("createdUtc")]
    public DateTime CreatedUtc { get; init; }

    public IEventPayload? GetEventPayload()
        => GetEventPayload(DefaultEventTypePayloadResolver.Instance);

    public IEventPayload? GetEventPayload(IEventTypePayloadResolver eventTypePayloadResolver)
    {
        var payloadType = eventTypePayloadResolver.GetEventPayloadType(StreamType, EventPayloadType);

        if (payloadType is null)
            throw new NullReferenceException($"Unable to resolve Payload type, using '{eventTypePayloadResolver.GetType().Name}'");
        
        return Payload.ToObject(payloadType) as IEventPayload;
    }
}