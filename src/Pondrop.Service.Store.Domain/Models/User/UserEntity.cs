using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.User;

namespace Pondrop.Service.Store.Domain.Models;

public record UserEntity : EventEntity
{
    public UserEntity()
    {
        Id = Guid.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
    }

    public UserEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }
    
    public UserEntity(string firstName, string lastName, string email) : this()
    {
        var createUser = new CreateUser(Guid.NewGuid(), firstName, lastName, email);
        Apply(createUser);
    }

    [JsonProperty(PropertyName = "firstName")]
    public string FirstName { get; private set; }
    
    [JsonProperty(PropertyName = "lastName")]
    public string LastName { get; private set; }

    [JsonProperty(PropertyName = "email")]
    public string Email { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateUser createUser:
                When(createUser);
                break;
            case UpdateName updateName:
                When(updateName);
                break;
            case UpdateEmail updateEmail:
                When(updateEmail);
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
        else if (eventPayloadToApply is CreateUser createUser)
        {
            Apply(new Event(GetStreamId<UserEntity>(createUser.Id), StreamType, 0, createUser));
        }
        else
        {
            throw new InvalidOperationException("Invalid event state");
        }
    }

    private void When(CreateUser createUser)
    {
        Id = createUser.Id;
        FirstName = createUser.FirstName;
        LastName = createUser.LastName;
        Email = createUser.Email;
    }

    private void When(UpdateName updateName)
    {
        FirstName = updateName.FirstName;
        LastName = updateName.LastName;
    }
    
    private void When(UpdateEmail updateEmail)
    {
        Email = updateEmail.Email;
    }
}