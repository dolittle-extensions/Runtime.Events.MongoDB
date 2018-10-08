/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Dynamic;
using MongoDB.Bson;
using Dolittle.Applications;
using Dolittle.Artifacts;
using Dolittle.Runtime.Events.Store;

namespace Dolittle.Runtime.Events.MongoDB
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
        /// <returns>A <see cref="CommittedEventStream" /> corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static CommittedEventStream ToCommittedEventStream(this BsonDocument doc)
        {
            return new CommittedEventStream(doc[Constants.ID].ToUlong(),
                                            doc.ToVersionedEventSource(),
                                            doc[CommitConstants.COMMIT_ID].ToCommitId(),
                                            doc[Constants.CORRELATION_ID].AsGuid,
                                            doc[CommitConstants.TIMESTAMP].AsDateTimeOffset(),
                                            doc[CommitConstants.EVENTS].ToEventStream());
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventMetadata" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <returns>An <see cref="EventMetadata" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static EventMetadata ToEventMetadata(this BsonDocument doc)
        {
            var eventId = doc[Constants.ID].AsGuid;
            var correlationId = doc[Constants.CORRELATION_ID].AsGuid;
            var event_artifact = doc[EventConstants.EVENT_ARTIFACT].AsGuid;
            var generation = doc[Constants.GENERATION].AsInt32;
            var origin = doc[EventConstants.ORIGINAL_CONTEXT].ToOriginalContext();
            var occurred = doc[EventConstants.OCCURRED].AsDateTimeOffset();
            return new EventMetadata(eventId,doc.ToVersionedEventSource(),correlationId,new Artifact(event_artifact,generation),occurred, origin);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="VersionedEventSource" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <returns>A <see cref="VersionedEventSource" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static VersionedEventSource ToVersionedEventSource(this BsonDocument doc)
        {
            var artifact = doc[Constants.EVENT_SOURCE_ARTIFACT].AsGuid;
            var eventSourceId = doc[Constants.EVENTSOURCE_ID].AsGuid;
            var major = doc[VersionConstants.COMMIT].ToUlong();
            var minor = doc[VersionConstants.SEQUENCE].ToUint();
            return new VersionedEventSource(new EventSourceVersion(major,minor),eventSourceId,artifact);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventSourceVersion" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <returns>A <see cref="EventSourceVersion" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static EventSourceVersion ToEventSourceVersion(this BsonDocument doc)
        {
            var major = doc[VersionConstants.COMMIT].ToUlong();
            var minor = doc[VersionConstants.SEQUENCE].ToUint();
            return new EventSourceVersion(major,minor);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventSourceVersion" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <returns>A <see cref="EventSourceVersion" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static CommittedEventVersion ToCommittedEventVersion(this BsonDocument doc)
        {
            var major = doc[Constants.MAJOR_VERSION].ToUlong();
            var minor = doc[Constants.MINOR_VERSION].ToUlong();
            var revision = doc[Constants.REVISION].ToUint();
            return new CommittedEventVersion(major,minor,revision);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument" /> representation into an <see cref="EventEnvelope" />
        /// </summary>
        /// <param name="doc">The <see cref="BsonDocument" /> representation</param>
        /// <returns>An <see cref="EventEnvelope" /> instance corresponding to the <see cref="BsonDocument" /> representation</returns>
        public static EventEnvelope ToEventEnvelope(this BsonDocument doc)
        {
            var eventDoc = doc[EventConstants.EVENT].AsBsonDocument;
           return new EventEnvelope(doc.ToEventMetadata(),eventDoc.ToPropertyBag());
        }
    }
}