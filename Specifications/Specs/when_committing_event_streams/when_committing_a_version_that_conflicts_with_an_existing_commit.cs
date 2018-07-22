using Machine.Specifications;
using System;

namespace Dolittle.Runtime.Events.Store.Specs.when_committing_event_streams
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_committing_a_version_that_conflicts_with_an_existing_commit : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream committed_events;
        static UncommittedEventStream uncommitted_events;
        static UncommittedEventStream conflicting_uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static Exception exception;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            var event_source = EventSourceId.New();
            uncommitted_events = event_source.BuildUncommitted(event_source_artifact, occurred);
            event_store.Commit(uncommitted_events);
            conflicting_uncommitted_events = event_source.BuildUncommitted(event_source_artifact, occurred);
        };

        Because of = () => event_store._do((es) => exception = Catch.Exception(() => es.Commit(conflicting_uncommitted_events)));

        It fails_as_the_commit_has_a_concurrency_conflict = () => exception.ShouldBeOfExactType<EventSourceConcurrencyConflict>();
    }
}