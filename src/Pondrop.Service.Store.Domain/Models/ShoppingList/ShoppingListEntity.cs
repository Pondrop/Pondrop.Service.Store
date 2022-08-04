using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.ShoppingList;
using Pondrop.Service.Store.Domain.Events.User;

namespace Pondrop.Service.Store.Domain.Models;

public record ShoppingListEntity : EventEntity
{
    private readonly List<ShoppingListItemEntity> _items = new List<ShoppingListItemEntity>();
    
    public ShoppingListEntity()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    public ShoppingListEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }
    
    public ShoppingListEntity(Guid userId, string name, string description) : this()
    {
        var createUser = new CreateShoppingList(Guid.NewGuid(), userId, name, description);
        Apply(createUser);
    }
    
    [JsonProperty(PropertyName = "userId")]
    public Guid UserId { get; private set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }
    
    [JsonProperty(PropertyName = "description")]
    public string Description { get; private set; }

    [JsonProperty(PropertyName = "items")]
    public List<ShoppingListItemEntity> Items => _items.ToList();
    
    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateShoppingList createList:
                When(createList);
                break;
            case UpdateShoppingList updateList:
                When(updateList);
                break;
            case AddItemToShoppingList addToList:
                When(addToList);
                break;
            case RemoveItemFromShoppingList removeFromList:
                When(removeFromList);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);
        AtSequence = eventToApply.SequenceNumber;
    }
    
    public sealed override void Apply(IEventPayload eventPayloadToApply)
    {
        if (Events.Any())
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply));
        }
        else if (eventPayloadToApply is CreateShoppingList createList)
        {
            Apply(new Event(GetStreamId<ShoppingListEntity>(createList.Id), StreamType, 0, createList));
        }
        else
        {
            throw new InvalidOperationException("Invalid event state");
        }
    }

    private void When(CreateShoppingList createList)
    {
        Id = createList.Id;
        UserId = createList.UserId;
        Name = createList.Name;
        Description = createList.Description;
    }
    
    private void When(UpdateShoppingList updateList)
    {
        Name = updateList.Name;
        Description = updateList.Description;
    }
    
    private void When(AddItemToShoppingList addItemToList)
    {
        _items.Add(new ShoppingListItemEntity(
            addItemToList.Id,
            Id,
            addItemToList.Name));
    }
    
    private void When(RemoveItemFromShoppingList removeItemFromList)
    {
        _items.RemoveAll(i => i.Id == removeItemFromList.Id);
    }
}