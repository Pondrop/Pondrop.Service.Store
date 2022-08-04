using System.Collections.Concurrent;
using System.Reflection;

namespace Pondrop.Service.Store.Domain.Events;

public class DefaultEventTypePayloadResolver : IEventTypePayloadResolver
{
    private static DefaultEventTypePayloadResolver? Default;
    public static IEventTypePayloadResolver Instance => Default ??= new DefaultEventTypePayloadResolver();
    
    private readonly ConcurrentDictionary<string, Type> _typeLookup = new ConcurrentDictionary<string, Type>();

    public Type? GetEventPayloadType(string streamType, string typeName)
    {
        if (!_typeLookup.TryGetValue(typeName, out var type))
        {
            type = Type.GetType($"{typeof(EventPayload).Namespace}.{streamType}.{typeName}, {typeof(IEventPayload).Assembly.FullName}");
            if (type is not null)
                _typeLookup.AddOrUpdate(typeName, type, (k, v) => type);
        }

        return type;
    }
}