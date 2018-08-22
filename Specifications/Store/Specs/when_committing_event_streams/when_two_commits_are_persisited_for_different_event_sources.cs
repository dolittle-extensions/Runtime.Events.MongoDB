using Machine.Specifications;
using Dolittle.Runtime.Events.Store.Specs;
using System;

namespace Dolittle.Runtime.Events.Store.Specs.when_committing_event_streams
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_two_commits_are_persisited_for_different_event_sources : given.an_event_store
    {
        static IEventStore event_store;
        static CommittedEventStream first_committed_events;
        static CommittedEventStream second_committed_events;
        static UncommittedEventStream first_uncommitted_events;
        static UncommittedEventStream second_uncommitted_events;
        static EventSourceId first_event_source_id;
        static EventSourceId second_event_source_id;
        static DateTimeOffset? occurred;

        Establish context = () => 
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow.AddSeconds(-10);
            first_uncommitted_events = EventSourceId.New().BuildUncommitted(event_source_artifact, occurred);
            second_uncommitted_events = EventSourceId.New().BuildUncommitted(event_source_artifact, occurred);
        };
        Because of = () => 
        {
            event_store._do((es) => first_committed_events = es.Commit(first_uncommitted_events));
            event_store._do((es) => second_committed_events = es.Commit(second_uncommitted_events));
        };
        It should_commit_the_event_streams_producing_a_committed_event_stream_for_each = () => 
        {
            first_committed_events.ShouldNotBeNull();
            second_committed_events.ShouldNotBeNull();
        };
        It should_return_a_committed_event_stream_corresponding_to_each_uncommitted_event_stream = () => 
        {
            first_committed_events.ShouldCorrespondTo(first_uncommitted_events);
            second_committed_events.ShouldCorrespondTo(second_uncommitted_events);
        };
        It should_return_committed_event_streams_with_a_unique_ids = () => 
        {
            first_committed_events.Id.ShouldNotBeNull();
            first_committed_events.Id.ShouldNotEqual(CommitId.Empty);  
            second_committed_events.Id.ShouldNotBeNull();
            second_committed_events.Id.ShouldNotEqual(CommitId.Empty); 
            first_committed_events.Id.ShouldNotEqual(second_committed_events.Id); 
        };
        It should_have_the_correct_versioned_event_source_for_each_stream = () => 
        {
            first_committed_events.Source.ShouldEqual(first_uncommitted_events.Source);
            second_committed_events.Source.ShouldEqual(second_uncommitted_events.Source);
        };
        It should_have_the_correct_sequence_id_for_the_each_commit = () => 
        {
            first_committed_events.Sequence.ShouldEqual(new CommitSequenceNumber(1));
            second_committed_events.Sequence.ShouldEqual(new CommitSequenceNumber(2));
        };
        Cleanup nh = () => event_store.Dispose();
    }
}