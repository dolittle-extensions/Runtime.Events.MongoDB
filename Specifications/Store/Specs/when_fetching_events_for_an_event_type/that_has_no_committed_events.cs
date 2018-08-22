using System;
using System.Linq;
using System.Collections.Generic;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs.when_fetching_events_for_an_event_type
{
    [Subject(typeof(IFetchCommittedEvents))]
    public class that_has_no_committed_events : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream first_commit;
        static CommittedEventStream second_commit;
        static UncommittedEventStream uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static SingleEventTypeEventStream result;

        Establish context = () => 
        {
            event_store = get_event_store();
            event_source_id = EventSourceId.New();
        };

        Because of = () => event_store._do((es) => result = es.FetchAllEventsOfType(event_artifacts[typeof(SimpleEvent)]));

        It should_retrieve_an_empty_event_stream = () => result.Any().ShouldBeFalse();
        
        Cleanup nh = () => event_store.Dispose();               
    }    
}