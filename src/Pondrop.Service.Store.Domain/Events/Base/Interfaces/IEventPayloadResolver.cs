namespace Pondrop.Service.Store.Domain.Events;

public interface IEventTypePayloadResolver
{
    Type? GetEventPayloadType(string streamType, string typeName);
}