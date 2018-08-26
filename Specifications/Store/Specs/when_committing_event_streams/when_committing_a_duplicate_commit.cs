using Machine.Specifications;
using System;

namespace Dolittle.Runtime.Events.Store.Specs.when_committing_event_streams
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_committing_a_duplicate_commit : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream committed_events;
        static UncommittedEventStream uncommitted_events;
        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static Exception exception;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            uncommitted_events = EventSourceId.New().BuildUncommitted(event_source_artifact, occurred);
            event_store.Commit(uncommitted_events);
        };

        Because of = () => event_store._do((es) => exception = Catch.Exception(() => es.Commit(uncommitted_events)));

        It fails_as_the_commit_is_a_duplicate = () => exception.ShouldBeOfExactType<CommitIsADuplicate>();

        Cleanup nh = () => event_store.Dispose();
    }
}