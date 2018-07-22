using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Dynamic;
using MongoDB.Bson;
using Dolittle.Applications;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Converts <see cref="BsonDocument" /> representations back into the original type and allows information to be easily extracted from the document
    /// </summary>
    public static class FromBsonDocumentExtensions 
    {
        /// <summary>
        /// Indicates whether the <see cref="BsonDocument" /> is the successful response to a commit
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> response to a commit</param>
        /// <returns>true if the commit was successful, otherwise false</returns>
        public static bool IsSuccessfulCommit(this BsonDocument doc)
        {
            return doc.Contains(Constants.ID);
        }

        /// <summary>
        /// Indicates whether the <see cref="BsonDocument" /> represents a known error condition
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> response to a commit</param>
        /// <returns>true if the document represents a known error, otherwise false</returns>
        public static bool IsKnownError(this BsonDocument doc)
        {
            return doc.Contains(Constants.ERROR);
        }

        /// <summary>
        /// Indicates whether the <see cref="BsonDocument" /> represents an error as the attempted committed version is an older version than the most recent
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> response to a commit</param>
        /// <returns>true if the document represents a previous version error, otherwise false</returns>
        public static bool IsPreviousVersion(this BsonDocument doc){
            return doc.Contains(Constants.ERROR) && doc.Contains(VersionConstants.COMMIT);
        }

        /// <summary>
        /// Indicates whether the <see cref="BsonDocument" /> represents a concurrency error
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> response to a commit</param>
        /// <returns>true if the document represents a concurrency error, otherwise false</returns>
        public static bool IsMongoConcurrencyError(this BsonDocument doc)
        {
            return doc["code"] == CommitConstants.CONCURRENCY_EXCEPTION;
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into a <see cref="CommittedEventStream" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <param name="converter">An <see cref="IApplicationArtifactIdentifierStringConverter" /> to handle deserialization of <see cref="IApplicationArtifactIdentifier" /></param>
        /// <returns>A <see cref="CommittedEventStream" /> corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static CommittedEventStream ToCommittedEventStream(this BsonDocument doc, IApplicationArtifactIdentifierStringConverter converter)
        {
            return new CommittedEventStream(doc[Constants.ID].ToUlong(),
                                            doc.ToVersionedEventSource(converter),
                                            doc[CommitConstants.COMMIT_ID].ToCommitId(),
                                            doc[Constants.CORRELATION_ID].AsGuid,
                                            doc[CommitConstants.TIMESTAMP].AsDateTimeOffset(),
                                            doc[CommitConstants.EVENTS].ToEventStream(converter));
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventMetadata" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <param name="converter">An <see cref="IApplicationArtifactIdentifierStringConverter" /> to handle deserialization of <see cref="IApplicationArtifactIdentifier" /></param>
        /// <returns>An <see cref="EventMetadata" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static EventMetadata ToEventMetadata(this BsonDocument doc, IApplicationArtifactIdentifierStringConverter converter)
        {
            var correlationId = doc[Constants.CORRELATION_ID].AsGuid;
            var event_artifact = doc[EventConstants.EVENT_ARTIFACT].AsString;
            var generation = doc[Constants.GENERATION].ToUint();
            var causedBy = doc[EventConstants.CAUSED_BY].AsString;
            var occurred = doc[EventConstants.OCCURRED].AsDateTimeOffset();
            return new EventMetadata(doc.ToVersionedEventSource(converter),correlationId,new ArtifactGeneration(converter.FromString(event_artifact),generation),causedBy,occurred);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="VersionedEventSource" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <param name="converter">An <see cref="IApplicationArtifactIdentifierStringConverter" /> to handle deserialization of <see cref="IApplicationArtifactIdentifier" /></param>
        /// <returns>A <see cref="VersionedEventSource" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static VersionedEventSource ToVersionedEventSource(this BsonDocument doc, IApplicationArtifactIdentifierStringConverter converter)
        {
            var artifact = doc[Constants.EVENT_SOURCE_ARTIFACT].AsString;
            var eventSourceId = doc[Constants.EVENTSOURCE_ID].AsGuid;
            var major = doc[VersionConstants.COMMIT].ToUlong();
            var minor = doc[VersionConstants.SEQUENCE].ToUint();
            return new VersionedEventSource(new EventSourceVersion(major,minor),eventSourceId,converter.FromString(artifact));
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventEnvelope" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <param name="converter">An <see cref="IApplicationArtifactIdentifierStringConverter" /> to handle deserialization of <see cref="IApplicationArtifactIdentifier" /></param>
        /// <returns>An <see cref="EventEnvelope" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static EventEnvelope ToEventEnvelope(this BsonDocument doc, IApplicationArtifactIdentifierStringConverter converter)
        {
            var eventDoc = doc[EventConstants.EVENT].AsBsonDocument;
           return new EventEnvelope(doc[Constants.ID].AsGuid,doc.ToEventMetadata(converter),eventDoc.ToPropertyBag());
        }
    }
}