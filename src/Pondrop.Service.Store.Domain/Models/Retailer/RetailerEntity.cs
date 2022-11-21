using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Store.Domain.Events.Retailer;

namespace Pondrop.Service.Store.Domain.Models;

public record RetailerEntity : EventEntity
{
    public RetailerEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        ExternalReferenceId = string.Empty;
    }

    public RetailerEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }
    
    public RetailerEntity(string name, string externalReferenceId, string createdBy) : this()
    {
        var create = new CreateRetailer(Guid.NewGuid(), externalReferenceId, name);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }
    
    [JsonProperty(PropertyName = "externalReferenceId")]
    public string ExternalReferenceId { get; private set; }
    
    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateRetailer create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateRetailer update:
                When(update);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);
        
        AtSequence = eventToApply.SequenceNumber;
        UpdatedBy = eventToApply.CreatedBy;
        UpdatedUtc = eventToApply.CreatedUtc;
    }
    
    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateRetailer create)
        {
            Apply(new Event(GetStreamId<RetailerEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateRetailer create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        ExternalReferenceId = create.ExternalReferenceId;
        CreatedBy = createdBy;
        CreatedUtc = createdUtc;
    }

    private void When(UpdateRetailer update)
    {
        Name = update.Name;
    }
}