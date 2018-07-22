using System;
using System.Linq;
using System.Collections.Generic;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs.when_retrieving_committed_event_streams
{
    [Subject(typeof(IFetchCommittedEvents))]
    public class when_fetching_events_for_an_event_source_that_has_no_commits : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream first_commit;
        static CommittedEventStream second_commit;
        static UncommittedEventStream uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static CommittedEvents result;

        Establish context = () => 
        {
            event_store = get_event_store();
            event_source_id = EventSourceId.New();
        };

        Because of = () => event_store._do((es) => result = es.Fetch(event_source_id));

        It should_retrieve_empty_commits = () => (result as IEnumerable<CommittedEventStream>).Count().ShouldEqual(0);
        
        Cleanup nh = () => event_store.Dispose();               
    }    
}