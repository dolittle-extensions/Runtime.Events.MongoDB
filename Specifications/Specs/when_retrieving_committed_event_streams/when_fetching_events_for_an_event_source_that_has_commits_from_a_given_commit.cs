using System;
using System.Linq;
using System.Collections.Generic;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs.when_retrieving_committed_event_streams
{

    [Subject(typeof(IFetchCommittedEvents))]
    public class when_fetching_events_for_an_event_source_that_has_commits_from_a_given_commit : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream first_commit;
        static CommittedEventStream second_commit;
        static CommittedEventStream third_commit;
        static UncommittedEventStream uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static CommittedEvents result;

        Establish context = () => 
        {
            event_store = get_event_store();
            event_source_id = EventSourceId.New();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            uncommitted_events = event_source_id.BuildUncommitted(event_source_artifact,occurred);
            event_store._do((es) => first_commit = es.Commit(uncommitted_events));
            uncommitted_events = first_commit.BuildNext(DateTimeOffset.UtcNow);
            event_store._do((es) => second_commit = es.Commit(uncommitted_events));
            uncommitted_events = second_commit.BuildNext(DateTimeOffset.UtcNow);
            event_store._do((es) => third_commit = es.Commit(uncommitted_events));
        };

        Because of = () => event_store._do((es) => result = es.FetchFrom(event_source_id, 2));

        It should_retrieve_all_the_commits_for_the_event_source_from_the_specified_version = () => result.Count().ShouldEqual(2);
        It should_retrieve_the_commits_in_order = () => 
        {
            result.First().Sequence.ShouldEqual(second_commit.Sequence);
            result.Last().Sequence.ShouldEqual(third_commit.Sequence);
        };
        It should_have_the_events_in_each_commit = () => 
        {
            result.First().Events.ShouldContainOnly(second_commit.Events);
            result.Last().Events.ShouldContainOnly(third_commit.Events);
        };
        Cleanup nh = () => event_store.Dispose();               
    }    
}