using Dolittle.Concepts;
using Dolittle.Events;

namespace Dolittle.Runtime.Events.Store.Specs
{
    public class AnotherSimpleEvent : Value<AnotherSimpleEvent>, IEvent
    {
        public AnotherSimpleEvent(EventId id, string contents, int count)
        {
            Id = id;
            Contents = contents;
            Count = count;
        }
        public AnotherSimpleEvent(string contents, int count) : this(EventId.New(),contents,count)
        {}
        public EventId Id { get; }
        public string Contents { get; }
        public int Count { get; }
    }    
}