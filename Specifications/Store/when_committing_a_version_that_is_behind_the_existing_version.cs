using Machine.Specifications;
using Dolittle.Runtime.Events.Store.Specs;
using System;
using Dolittle.Runtime.Events.Store.MongoDB.Specs;

namespace Dolittle.Runtime.Events.Store
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_committing_a_version_that_is_behind_the_existing_version_and_the_version_is_out_of_sync : Specs.given.an_event_store
    {
        static IEventStore event_store;
        static UncommittedEventStream behind_uncommitted_events;
        static UncommittedEventStream latest_uncommitted_events;
        static DateTimeOffset? occurred;
        static Exception exception;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            var event_source = EventSourceId.New();
            behind_uncommitted_events = event_source.BuildUncommitted(event_source_artifact, occurred);
            latest_uncommitted_events = behind_uncommitted_events.BuildNext(occurred);
            event_store._do(_ => _.Commit(latest_uncommitted_events));
            event_store._do(_ => (_ as test_mongodb_event_store).TestUpdateVersion(behind_uncommitted_events.Source));
        };

        Because of = () => event_store._do((es) => exception = Catch.Exception(() => es.Commit(behind_uncommitted_events)));

        It fails_as_the_commit_has_a_concurrency_conflict = () => exception.ShouldBeOfExactType<EventSourceConcurrencyConflict>();
        It should_update_the_version_to_the_last_version_from_the_commits = () =>  event_store._do(_ => _.GetCurrentVersionFor(latest_uncommitted_events.Source.EventSource).ShouldEqual(latest_uncommitted_events.Source.Version));

        Cleanup nh = () => event_store.Dispose();
    }
}