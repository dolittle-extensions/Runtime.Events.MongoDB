using Machine.Specifications;
using Dolittle.Runtime.Events.Store.Specs;
using System;

namespace Dolittle.Runtime.Events.Store.Specs.when_committing_event_streams
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_committing_a_version_that_is_ahead_of_the_next_expected_version : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream committed_events;
        static UncommittedEventStream ahead_uncommitted_events;

        static EventSourceId event_source_id;
        static DateTimeOffset? occurred;
        static Exception exception;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            var event_source = EventSourceId.New();
            event_source.BuildUncommitted(event_source_artifact,occurred);
            ahead_uncommitted_events = event_source.BuildUncommitted(event_source_artifact, occurred);
        };

        Because of = () => event_store._do((es) => committed_events = es.Commit(ahead_uncommitted_events));

        It should_commit_the_events_producing_a_committed_event_stream = () => committed_events.ShouldNotBeNull();
        It should_return_a_committed_event_stream_corresponding_to_the_uncommitted_event_stream = () => 
        {
            committed_events.ShouldCorrespondTo(ahead_uncommitted_events);
        };
        It should_return_a_committed_event_stream_with_a_unique_id = () => 
        {
            committed_events.Id.ShouldNotBeNull();
            committed_events.Id.ShouldNotEqual(CommitId.Empty);  
        };
        It should_have_the_correct_versioned_event_source = () => committed_events.Source.ShouldEqual(ahead_uncommitted_events.Source);
        It should_have_a_sequence_id = () => committed_events.Sequence.ShouldEqual(new CommitSequenceNumber(1));
        Cleanup nh = () => event_store.Dispose();
    }
}