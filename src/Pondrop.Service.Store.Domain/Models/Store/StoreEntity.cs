using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Store.Domain.Events.Store;
using System.Spatial;

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
        RetailerId = Guid.Empty;
        StoreTypeId = Guid.Empty;
        IsCommunityStore = false;
    }

    public StoreEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public StoreEntity(string name, string status, string externalReferenceId, string phone, string email, string openHours, bool isCommunityStore, Guid retailerId, Guid storeTypeId, string createdBy) : this()
    {
        var create = new CreateStore(Guid.NewGuid(), name, status, phone, email, openHours, externalReferenceId, isCommunityStore, retailerId, storeTypeId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "status")]
    public string Status { get; private set; }

    [JsonProperty(PropertyName = "phone")]
    public string Phone { get; private set; }

    [JsonProperty(PropertyName = "email")]
    public string Email { get; private set; }

    [JsonProperty(PropertyName = "openHours")]
    public string OpenHours { get; private set; }

    [JsonProperty(PropertyName = "isCommunityStore")]
    public bool IsCommunityStore { get; private set; }

    [JsonProperty(PropertyName = "externalReferenceId")]
    public string ExternalReferenceId { get; private set; }

    [JsonProperty(PropertyName = "addresses")]
    public List<StoreAddressRecord> Addresses { get; private set; }

    [JsonProperty(PropertyName = "retailerId")]
    public Guid RetailerId { get; private set; }

    [JsonProperty(PropertyName = "storeTypeId")]
    public Guid StoreTypeId { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateStore create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStore update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case AddStoreAddress addAddress:
                When(addAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStoreAddress updateAddress:
                When(updateAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case RemoveAddressFromStore removeAddress:
                When(removeAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
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
        Phone = create.Phone;
        Email = create.Email;
        OpenHours = create.OpenHours;
        ExternalReferenceId = create.ExternalReferenceId;
        IsCommunityStore = create.IsCommunityStore;
        RetailerId = create.RetailerId;
        StoreTypeId = create.StoreTypeId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateStore update, string createdBy, DateTime createdUtc)
    {
        var oldName = Name;
        var oldStatus = Status;
        var oldRetailerId = RetailerId;
        var oldStoreTypeId = StoreTypeId;
        var oldIsCommunityStore = IsCommunityStore;

        Name = update.Name ?? Name;
        Status = update.Status ?? Status;
        Phone = update.Phone ?? Phone;
        Email = update.Email ?? Email;
        OpenHours = update.OpenHours ?? OpenHours;
        RetailerId = update.RetailerId ?? RetailerId;
        StoreTypeId = update.StoreTypeId ?? StoreTypeId;
        IsCommunityStore = update.IsCommunityStore;

        if (oldName != Name ||
            oldStatus != Status ||
            oldRetailerId != StoreTypeId ||
            oldIsCommunityStore != IsCommunityStore ||
            oldStoreTypeId != StoreTypeId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
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
            new Point(addAddress.Longitude, addAddress.Latitude),
            createdBy,
            createdBy,
            createdUtc,
            createdUtc));

        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
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
            Longitude = updateAddress.Longitude ?? address.Longitude,
            UpdatedBy = updatedBy,
            UpdatedUtc = updatedUtc,
            LocationSort = new Point(updateAddress.Longitude ?? address.Longitude, updateAddress.Latitude ?? address.Latitude),
        };


        UpdatedBy = updatedBy;
        UpdatedUtc = updatedUtc;
    }

    private void When(RemoveAddressFromStore removeItemFromList, string updatedBy, DateTime updatedUtc)
    {
        var address = Addresses.FirstOrDefault(i => i.Id == removeItemFromList.Id);
        Addresses = new List<StoreAddressRecord>();

        UpdatedBy = updatedBy;
        UpdatedUtc = updatedUtc;
    }
}