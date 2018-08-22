using System;
using System.Linq;
using System.Collections.Generic;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs.when_fetching_events_for_an_event_type
{
    [Subject(typeof(IFetchCommittedEvents))]
    public class that_has_events_from_a_given_commit_greater_than_committed : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream first_commit;
        static CommittedEventStream second_commit;
        static CommittedEventStream third_commit;
        static UncommittedEventStream uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static SingleEventTypeEventStream result;

        Establish context = () => 
        {
            event_store = get_event_store();
            event_source_id = EventSourceId.New();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            uncommitted_events = event_source_id.BuildUncommitted(event_source_artifact, occurred);
            event_store._do((es) => first_commit = es.Commit(uncommitted_events));
            uncommitted_events = first_commit.BuildNext(DateTimeOffset.UtcNow);
            event_store._do((es) => second_commit = es.Commit(uncommitted_events));
            uncommitted_events = second_commit.BuildNext(DateTimeOffset.UtcNow);
            event_store._do((es) => third_commit = es.Commit(uncommitted_events));
        };

        Because of = () => event_store._do((es) => result = es.FetchAllEventsOfTypeAfter(event_artifacts[typeof(SimpleEvent)],4));

        It should_retrieve_no_events = () => result.Count().ShouldEqual(0);
        Cleanup nh = () => event_store.Dispose();               
    }      
}