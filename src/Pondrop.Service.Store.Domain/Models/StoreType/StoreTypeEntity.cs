using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Store.Domain.Events.StoreType;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreTypeEntity : EventEntity
{
    public StoreTypeEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        ExternalReferenceId = string.Empty;
    }

    public StoreTypeEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }
    
    public StoreTypeEntity(string name, string externalReferenceId, string createdBy) : this()
    {
        var create = new CreateStoreType(Guid.NewGuid(), externalReferenceId, name);
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
            case CreateStoreType create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStoreType update:
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
        if (eventPayloadToApply is CreateStoreType create)
        {
            Apply(new Event(GetStreamId<StoreTypeEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateStoreType create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        ExternalReferenceId = create.ExternalReferenceId;
        CreatedBy = createdBy;
        CreatedUtc = createdUtc;
    }

    private void When(UpdateStoreType update)
    {
        Name = update.Name;
    }
}