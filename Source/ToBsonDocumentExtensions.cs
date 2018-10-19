/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Applications;
using Dolittle.PropertyBags;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Processing;

namespace Dolittle.Runtime.Events.MongoDB
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
                    { EventConstants.EVENT_ARTIFACT, e.Metadata.Artifact.Id.Value },
                    { Constants.GENERATION, e.Metadata.Artifact.Generation.Value },
                    { Constants.EVENT_SOURCE_ARTIFACT, uncommittedEvents.Source.Artifact.Value },
                    { Constants.EVENTSOURCE_ID, e.Metadata.EventSourceId.Value },
                    { VersionConstants.COMMIT, e.Metadata.VersionedEventSource.Version.Commit},
                    { VersionConstants.SEQUENCE, e.Metadata.VersionedEventSource.Version.Sequence},
                    { EventConstants.OCCURRED, e.Metadata.Occurred.UtcTicks },
                    { EventConstants.ORIGINAL_CONTEXT, e.Metadata.OriginalContext.AsBson()},
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
        /// Converts a <see cref="CommittedEventVersion" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="version">The <see cref="CommittedEventVersion" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="CommittedEventVersion" /></returns>
        public static BsonDocument AsBson(this CommittedEventVersion version )
        {
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.MAJOR_VERSION, version.Major },
                { Constants.MINOR_VERSION, version.Minor },
                { Constants.REVISION, version.Revision }
            });
        }

        /// <summary>
        /// Converts a <see cref="EventSourceId" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="eventSourceId">The <see cref="EventSourceId" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="EventSourceId" /></returns>
        public static BsonDocument AsBson(this EventSourceId eventSourceId )
        {
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.EVENTSOURCE_ID,eventSourceId.Value }
            });
        }

        /// <summary>
        /// Converts an <see cref="EventProcessorId" /> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="id">The <see cref="EventProcessorId" /></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="EventProcessorId" /></returns>
        public static BsonDocument AsBson(this EventProcessorId id )
        {
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.ID, id.Value }
            });
        }        

        /// <summary>
        /// Converts a <see cref="FilterDefinition{T}"/> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="filter">The <see cref="FilterDefinition{T}"/></param>
        /// <typeparam name="T">type of the filter definition</typeparam>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="FilterDefinition{T}" /></returns>
        public static BsonDocument AsBson<T>(this FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry);
        }

        /// <summary>
        /// Converts an <see cref="OriginalContext"/> into its <see cref="BsonDocument" /> representation
        /// </summary>
        /// <param name="originalContext">The <see cref="OriginalContext"/></param>
        /// <returns>A <see cref="BsonDocument" /> representation of the <see cref="OriginalContext"/></returns>
        public static BsonDocument AsBson(this OriginalContext originalContext)
        {
            var claims = new BsonArray();
            claims.AddRange(originalContext.Claims.Select(_ => _.ToBsonDocument()));

            return new BsonDocument( new Dictionary<string,object>
            {
                { EventConstants.APPLICATION, originalContext.Application.Value },
                { EventConstants.BOUNDED_CONTEXT, originalContext.BoundedContext.Value },
                { EventConstants.TENANT, originalContext.Tenant.Value },
                { EventConstants.ENVIRONMENT, originalContext.Environment.Value },
                { EventConstants.CLAIMS, claims }
            });
        }
    }
}