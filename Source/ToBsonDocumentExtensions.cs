using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Dynamic;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Extensions to convert various classes into their <see cref="BsonDocument" /> representation
    /// </summary>
    public static class ToBsonDocumentExtensions
    {
        /// <summary>
        /// Converts an <see cref="UncommittedEventStream" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="uncommittedEvents">The <see cref="UncommittedEventStream" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="UncommittedEventStream" /></returns>
        public static BsonDocument AsBsonCommit(this UncommittedEventStream uncommittedEvents)
        {
            var eventDocs = uncommittedEvents.Events.Select(e => 
            {
                return new BsonDocument( new Dictionary<string,object>
                {
                    { Constants.ID, e.Id.Value },
                    { Constants.CORRELATION_ID, e.Metadata.CorrelationId.Value },
                    { EventConstants.EVENT_ARTIFACT, e.Metadata.ArtifactGeneration.Artifact.Value },
                    { Constants.GENERATION, e.Metadata.ArtifactGeneration.Generation.Value },
                    { Constants.EVENT_SOURCE_ARTIFACT, uncommittedEvents.Source.Artifact.Value },
                    { Constants.EVENTSOURCE_ID, e.Metadata.EventSourceId.Value },
                    { EventConstants.CAUSED_BY, e.Metadata.CausedBy.Value ?? string.Empty },
                    { VersionConstants.COMMIT, e.Metadata.VersionedEventSource.Version.Commit},
                    { VersionConstants.SEQUENCE, e.Metadata.VersionedEventSource.Version.Sequence},
                    { EventConstants.OCCURRED, e.Metadata.Occurred.UtcTicks },
                    { EventConstants.EVENT, BsonDocumentWrapper.Create<PropertyBag>(e.Event) }
                });
            });
                    
            var doc = new BsonDocument(new Dictionary<string,object>
            { 
                { Constants.ID, 0 },
                { Constants.CORRELATION_ID, uncommittedEvents.CorrelationId.Value },
                { CommitConstants.COMMIT_ID, uncommittedEvents.Id.Value },
                { CommitConstants.TIMESTAMP, uncommittedEvents.Timestamp.ToUnixTimeMilliseconds() },
                { Constants.EVENTSOURCE_ID, uncommittedEvents.Source.EventSource.Value },
                { Constants.EVENT_SOURCE_ARTIFACT, uncommittedEvents.Source.Artifact.Value },
                { VersionConstants.COMMIT, uncommittedEvents.Source.Version.Commit},
                { VersionConstants.SEQUENCE, uncommittedEvents.Source.Version.Sequence},
                { CommitConstants.EVENTS, new BsonArray(eventDocs) },     
            });
            return doc;
        }

        /// <summary>
        /// Converts a <see cref="VersionedEventSource" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="version">The <see cref="VersionedEventSource" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="VersionedEventSource" /></returns>
        public static BsonDocument AsBsonVersion(this VersionedEventSource version )
        {
            //expand to hold reference to snapshot, maybe event count for a snapshotting process
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.EVENTSOURCE_ID, version.EventSource.Value },
                { VersionConstants.COMMIT, version.Version.Commit },
                { VersionConstants.SEQUENCE, version.Version.Sequence }
            });
        }

        /// <summary>
        /// Converts a <see cref="EventSourceId" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="eventSourceId">The <see cref="EventSourceId" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="EventSourceId" /></returns>
        public static BsonDocument AsBson(this EventSourceId eventSourceId )
        {
            //expand to hold reference to snapshot, maybe event count for a snapshotting process
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.EVENTSOURCE_ID,eventSourceId.Value }
            });
        }
    }

}