// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Security;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Specs.MongoDB
{
    [Subject(typeof(ICommitEventStreams))]
    public class when_persisting_event_context : Store.Specs.given.an_event_store
    {
        static CommittedEventStream committed_events;
        static UncommittedEventStream uncommitted_events;
        static DateTimeOffset? occurred;
        static IEventStore event_store;

        Establish context = () =>
        {
            event_store = get_event_store();
            occurred = DateTimeOffset.UtcNow;
            uncommitted_events = build_new_uncommitted_event_stream_from(get_event_source_key()
                                                                            .BuildUncommitted(occurred));
        };

        Because of = () => event_store._do(
            (event_store) =>
            {
                event_store.Commit(uncommitted_events);
                var commits = event_store.FetchAllCommitsAfter(0);
                committed_events = commits.First();
            });

        It should_have_claims_in_the_uncommitted_events = () => uncommitted_events.Events.First().Metadata
                                                                    .OriginalContext.Claims.Any()
                                                                    .ShouldBeTrue();

        It should_not_have_claims_in_the_committed_events = () => committed_events.Events.First().Metadata
                                                                        .OriginalContext.Claims.Any()
                                                                        .ShouldBeFalse();

        Cleanup nh = () => event_store.Dispose();

        static UncommittedEventStream build_new_uncommitted_event_stream_from(UncommittedEventStream uncommitted)
        {
            return new UncommittedEventStream(
                            uncommitted.Id,
                            uncommitted.CorrelationId,
                            uncommitted.Source,
                            uncommitted.Timestamp,
                            build_new_event_stream_from(uncommitted.Events));
        }

        static EventStream build_new_event_stream_from(EventStream eventStream)
        {
            return new EventStream(eventStream.Select(build_new_event_envelope_from));
        }

        static EventEnvelope build_new_event_envelope_from(EventEnvelope envelope)
        {
            return new EventEnvelope(build_new_metadata_from(envelope.Metadata), envelope.Event);
        }

        static EventMetadata build_new_metadata_from(EventMetadata metadata)
        {
            return new EventMetadata(
                        metadata.Id,
                        metadata.VersionedEventSource,
                        metadata.CorrelationId,
                        metadata.Artifact,
                        metadata.Occurred,
                        build_new_original_context(metadata.OriginalContext));
        }

        static OriginalContext build_new_original_context(OriginalContext context)
        {
            return new OriginalContext(
                        context.Application,
                        context.BoundedContext,
                        context.Tenant,
                        context.Environment,
                        build_claims(),
                        context.CommitInOrigin);
        }

        static Claims build_claims()
        {
            return new Claims(new[]
            {
                new Claim("Test", "Test", "Test"),
                new Claim("Date", "Date", "Date")
            });
        }
    }
}