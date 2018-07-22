using Machine.Specifications;
using Dolittle.Runtime.Events.Store.Specs;
using System;

namespace Dolittle.Runtime.Events.Store.Specs.when_committing_event_streams
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_committing_a_version_that_is_behind_the_existing_version : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream committed_events;
        static UncommittedEventStream behind_uncommitted_events;
        static UncommittedEventStream latest_uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static Exception exception;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            var event_source = EventSourceId.New();
            behind_uncommitted_events = event_source.BuildUncommitted(event_source_artifact, occurred);
            latest_uncommitted_events = behind_uncommitted_events.BuildNext(occurred);
            event_store.Commit(latest_uncommitted_events);
        };

        Because of = () => event_store._do((es) => exception = Catch.Exception(() => es.Commit(behind_uncommitted_events)));

        It fails_as_the_commit_has_a_concurrency_conflict = () => exception.ShouldBeOfExactType<EventSourceConcurrencyConflict>();
    }
}