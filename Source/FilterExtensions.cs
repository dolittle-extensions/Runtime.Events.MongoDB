/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
 
using MongoDB.Driver;
using MongoDB.Bson;
using Dolittle.Artifacts;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Relativity;
using Dolittle.Runtime.Events.Relativity.MongoDB;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Extensions for converting to a <see cref="FilterDefinition{BsonDocument}" />
    /// </summary>
    public static class FilterExtensions
    {
        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventSourceKey" /> supplied
        /// </summary>
        /// <param name="key">An <see cref="EventSourceKey" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventSourceKey" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this EventSourceKey key)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.EVENTSOURCE_ID, key.Id.Value) & builder.Eq(Constants.EVENT_SOURCE_ARTIFACT, key.Artifact.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitId" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitId" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitId" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitId commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(CommitConstants.COMMIT_ID, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitVersion" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitVersion" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitVersion" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitVersion commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Gte(VersionConstants.COMMIT, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitSequenceNumber" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitSequenceNumber" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitSequenceNumber" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitSequenceNumber commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Gt(Constants.ID, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventProcessorId" /> supplied
        /// </summary>
        /// <param name="id">A <see cref="EventProcessorId" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventProcessorId" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this EventProcessorId id)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.ID, id.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventHorizonKey" /> supplied
        /// </summary>
        /// <param name="key">An <see cref="EventHorizonKey" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventHorizonKey" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this EventHorizonKey key)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.ID, key.AsId());
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="VersionedEventSource" /> supplied
        /// </summary>
        /// <param name="version">A <see cref="VersionedEventSource" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="VersionedEventSource" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this VersionedEventSource version)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.EVENTSOURCE_ID, version.EventSource.Value) & 
                            builder.Eq(VersionConstants.COMMIT, version.Version.Commit);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="ArtifactId" /> supplied
        /// </summary>
        /// <param name="eventType">An <see cref="ArtifactId" /> of an event type</param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="ArtifactId" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this ArtifactId eventType)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.QUERY_EVENT_ARTIFACT, eventType.Value);
            return filter;
        }
    }
}