using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.Store;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreEntity : EventEntity
{
    public StoreEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        Status = string.Empty;
        ExternalReferenceId = string.Empty;
        Addresses = new List<StoreAddressRecord>();
        Retailer = new RetailerRecord();
        StoreType = new StoreTypeRecord();
    }

    public StoreEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }
    
    public StoreEntity(string name, string status, string externalReferenceId, RetailerRecord retailer, StoreTypeRecord storeType, string createdBy) : this()
    {
        var create = new CreateStore(Guid.NewGuid(), name, status, externalReferenceId, retailer, storeType);
        Apply(create, createdBy);
    }
    
    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }
    
    [JsonProperty(PropertyName = "status")]
    public string Status { get; private set; }
    
    [JsonProperty(PropertyName = "externalReferenceId")]
    public string ExternalReferenceId { get; private set; }

    [JsonProperty(PropertyName = "addresses")]
    public List<StoreAddressRecord> Addresses { get; private set; }
    
    [JsonProperty(PropertyName = "retailer")]
    public RetailerRecord Retailer { get; private set; }

    [JsonProperty(PropertyName = "storeType")]
    public StoreTypeRecord StoreType { get; private set; }
    
    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateStore create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStore update:
                When(update);
                break;
            case AddStoreAddress addAddress:
                When(addAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStoreAddress updateAddress:
                When(updateAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case RemoveAddressFromStore removeAddress:
                When(removeAddress);
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
        if (eventPayloadToApply is CreateStore create)
        {
            Apply(new Event(GetStreamId<StoreEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateStore create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        Status = create.Status;
        ExternalReferenceId = create.ExternalReferenceId;
        Retailer = create.Retailer;
        StoreType = create.StoreType;
        CreatedBy = createdBy;
        CreatedUtc = createdUtc;
    }
    
    private void When(UpdateStore update)
    {
        Name = update.Name ?? Name;
        Status = update.Status ?? Status;
        Retailer = update.Retailer ?? Retailer;
        StoreType = update.StoreType ?? StoreType;
    }
    
    private void When(AddStoreAddress addAddress, string createdBy, DateTime createdUtc)
    {
        Addresses.Add(new StoreAddressRecord(
            addAddress.Id,
            addAddress.ExternalReferenceId,
            addAddress.AddressLine1,
            addAddress.AddressLine2,
            addAddress.Suburb,
            addAddress.State,
            addAddress.Postcode,
            addAddress.Country,
            addAddress.Latitude,
            addAddress.Longitude,
            createdBy,
            createdBy,
            createdUtc,
            createdUtc));
    }
    
    private void When(UpdateStoreAddress updateAddress, string updatedBy, DateTime updatedUtc)
    {
        var address = Addresses.Single(i => i.Id == updateAddress.Id);
        var idx = Addresses.IndexOf(address);
        
        Addresses[idx] = address with
        {
            AddressLine1 = updateAddress.AddressLine1 ?? address.AddressLine1,
            AddressLine2 = updateAddress.AddressLine2 ?? address.AddressLine2,
            Suburb = updateAddress.Suburb ?? address.Suburb,
            State = updateAddress.State ?? address.State,
            Postcode = updateAddress.Postcode ?? address.Postcode,
            Country = updateAddress.Country ?? address.Country,
            Latitude = updateAddress.Latitude ?? address.Latitude,
            Longitude = updateAddress.Latitude ?? address.Longitude,
            UpdatedBy = updatedBy,
            UpdatedUtc = updatedUtc
        };
    }
    
    private void When(RemoveAddressFromStore removeItemFromList)
    {
        var address = Addresses.Single(i => i.Id == removeItemFromList.Id);
        Addresses.Remove(address);
    }
}