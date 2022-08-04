using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Store.Domain.Events;

public class EventStream
{
    private readonly SortedSet<IEvent>? _events;

    public EventStream(string streamId)
    {
        _events = null;
        StreamId = streamId;
    }
    
    public EventStream(string streamId, IEnumerable<IEvent> events) : this(streamId)
    {
        _events = new SortedSet<IEvent>(events, new EventSequenceComparer());
        Version = _events.LastOrDefault()?.SequenceNumber ?? -1;
    }
    
    public string StreamId { get; private set; }
    public long Version { get; private set; }

    public IEnumerable<IEvent> Events => _events?.AsEnumerable() ?? Enumerable.Empty<IEvent>();

    private class EventSequenceComparer : IComparer<IEvent>
    {
        public int Compare(IEvent? x, IEvent? y)
        {
            var cmp = (x?.SequenceNumber ?? -1).CompareTo(y?.SequenceNumber ?? -1);
            return cmp;
        }
    } 
}