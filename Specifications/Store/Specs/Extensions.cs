using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using System.Linq;
using Dolittle.PropertyBags;
using Dolittle.Artifacts;
using Dolittle.Events;
using Dolittle.Runtime.Events.Store;
using Dolittle.Collections;

namespace Dolittle.Runtime.Events.Store.Specs
{
    public static class Extensions
    {   
        public static void _do(this IEventStore event_store, Action<IEventStore> @do)
        {
            try
            {
                @do(event_store);
            }
            catch (System.Exception)
            {
                (event_store).Dispose();
                throw;
            }
        }

        public static UncommittedEventStream BuildUncommitted(this EventSourceId eventSourceId, ArtifactId eventSourceArtifact, DateTimeOffset? now = null, CorrelationId correlationId = null)
        {
            var committed = now ?? DateTimeOffset.Now;
            var events = BuildEvents();
            VersionedEventSource versionedEventSource = eventSourceId.InitialVersion(eventSourceArtifact);
            return BuildFrom(versionedEventSource,committed,correlationId ?? Guid.NewGuid(),events);
            
        }

        private static UncommittedEventStream BuildFrom(VersionedEventSource version, DateTimeOffset committed, CorrelationId correlationId, IEnumerable<IEvent> events)
        {
            var envelopes = new List<EventEnvelope>();
            VersionedEventSource vsn = null;
            events.ForEach(e => 
            {
                vsn = vsn == null ? version : new VersionedEventSource(vsn.Version.IncrementSequence(),vsn.EventSource,vsn.Artifact);
                envelopes.Add(e.ToEnvelope(EventId.New(),BuildEventMetadata(vsn, e.ToArtifact().Initial(), correlationId, committed)));
            });

            if(envelopes == null || !envelopes.Any())
                throw new ApplicationException("There are no envelopes");
            return BuildStreamFrom(envelopes.ToEventStream());
        }

        private static UncommittedEventStream BuildFrom(VersionedEventSource version, DateTimeOffset committed, CorrelationId correlationId, IEnumerable<EventEnvelope> events)
        {   
            var envelopes = new List<EventEnvelope>();
            VersionedEventSource vsn = null;
            events.ForEach(e => 
            {
                vsn = vsn == null ? version : new VersionedEventSource(vsn.Version.IncrementSequence(),vsn.EventSource,vsn.Artifact);
                envelopes.Add(e.ToNewEnvelope(vsn,committed,correlationId));
            });

            if(envelopes == null || !envelopes.Any())
                throw new ApplicationException("There are no envelopes");
            return BuildStreamFrom(envelopes.ToEventStream());
        }

        public static UncommittedEventStream BuildNext(this UncommittedEventStream eventStream, DateTimeOffset? now = null, CorrelationId correlationId = null)
        {
            Ensure.IsNotNull("eventStream", eventStream);
            Ensure.ArgumentPropertyIsNotNull("eventStream","Source",eventStream.Source);
            var committed = now ?? DateTimeOffset.Now;
            var eventSourceVersion = eventStream.Source.Next();
            return BuildFrom(eventSourceVersion,committed,correlationId ?? Guid.NewGuid(), eventStream.Events);
        }

       public static UncommittedEventStream BuildNext(this CommittedEventStream eventStream, DateTimeOffset? now = null, CorrelationId correlationId = null)
        {
            Ensure.IsNotNull("eventStream", eventStream);
            Ensure.ArgumentPropertyIsNotNull("eventStream","Source",eventStream.Source);
            var committed = now ?? DateTimeOffset.Now;
            var eventSourceVersion = eventStream.Source.Next();
            return BuildFrom(eventSourceVersion,committed,correlationId ?? Guid.NewGuid(), eventStream.Events);
        }

        private static EventMetadata BuildEventMetadata(VersionedEventSource versionedEventSource, Artifact artifact, CorrelationId correlationId, DateTimeOffset committed)
        {
            return new EventMetadata(versionedEventSource, correlationId, artifact, "A Test", committed);
        }

        private static UncommittedEventStream BuildStreamFrom(EventStream stream)
        {
            var now = DateTimeOffset.UtcNow;
            var lastEvent = stream.Last();
            var versionedEventSource = lastEvent.Metadata.VersionedEventSource;
            var correlationId = lastEvent.Metadata.CorrelationId;
            return new UncommittedEventStream(CommitId.New(), correlationId, versionedEventSource, now, stream);
        }

        static IEnumerable<IEvent> BuildEvents()
        {
            yield return new SimpleEvent("First",1);
            yield return new AnotherSimpleEvent("Second",2);
            yield return new SimpleEvent("Third",3);
            yield return new AnotherSimpleEvent("Fourth",4);
        }

        public static VersionedEventSource InitialVersion(this EventSourceId eventSourceId, ArtifactId artifact)
        {
            return new VersionedEventSource(EventSourceVersion.Initial(), eventSourceId, artifact);
        }

        public static VersionedEventSource Next(this VersionedEventSource version)
        {
            Ensure.IsNotNull("version",version);
            Ensure.ArgumentPropertyIsNotNull("version","Version",version.Version);
            Ensure.ArgumentPropertyIsNotNull("version","EventSource",version.EventSource);
            return new VersionedEventSource(version.Version.Next(), version.EventSource, version.Artifact);
        }

        public static EventStream ToEventStream(this IEnumerable<EventEnvelope> envelopes)
        {
            return EventStream.From(envelopes);
        }

        public static EventSourceVersion Next(this EventSourceVersion version)
        {
            return new EventSourceVersion((version.Commit + 1),0);
        }

        public static Artifact Initial(this ArtifactId artifact)
        {
            return new Artifact(artifact,ArtifactGeneration.First);
        }

        public static ArtifactId ToArtifact(this IEvent @event)
        {
            return given.an_event_store.event_artifacts.GetOrAdd(@event.GetType(),Guid.NewGuid());
        }

        public static EventEnvelope ToNewEnvelope(this EventEnvelope envelope, VersionedEventSource versionedEventSource, DateTimeOffset committed, CorrelationId correlationId)
        {
            return new EventEnvelope(EventId.New(),new EventMetadata(versionedEventSource,correlationId,envelope.Metadata.Artifact,envelope.Metadata.CausedBy,committed),envelope.Event);
        }
    }
}