// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using Dolittle.Runtime.Events.Store;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Processing.MongoDB
{
    /// <summary>
    /// Represents an implementation of <see cref="IEventProcessorOffsetRepository"/> for MongoDB.
    /// </summary>
    public class EventProcessorOffsetRepository : IEventProcessorOffsetRepository
    {
        const string OFFSETS = "offsets";
        readonly IMongoDatabase _database;
        readonly ILogger _logger;
        readonly MongoCollectionSettings _offsetsSettings;
        readonly IMongoCollection<BsonDocument> _offsets;
        bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProcessorOffsetRepository"/> class.
        /// </summary>
        /// <param name="connection"><see cref="Connection"/> to the MongoDB.</param>
        /// <param name="logger"><see cref="ILogger"/> for logging.</param>
        public EventProcessorOffsetRepository(Connection connection, ILogger logger)
        {
            _database = connection.Database;
            _logger = logger;
            _offsetsSettings = new MongoCollectionSettings { AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
            _offsets = _database.GetCollection<BsonDocument>(OFFSETS, _offsetsSettings);
        }

        /// <inheritdoc/>
        public CommittedEventVersion Get(EventProcessorId eventProcessorId)
        {
            var version = _offsets.Find(eventProcessorId.ToFilter()).SingleOrDefault();
            if (version == null)
                return CommittedEventVersion.None;

            return version.ToCommittedEventVersion();
        }

        /// <inheritdoc/>
        public void Set(EventProcessorId eventProcessorId, CommittedEventVersion committedEventVersion)
        {
            var versionBson = committedEventVersion.AsBson();
            versionBson.Add(Constants.ID, eventProcessorId.Value);
            _offsets.ReplaceOne(eventProcessorId.ToFilter(), versionBson, new UpdateOptions { IsUpsert = true });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <summary>
        /// Helper function to perform an action and return the results.
        /// </summary>
        /// <param name="callback">Action to be performed.</param>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <returns>Instance of T.</returns>
        protected virtual T Do<T>(Func<T> callback)
        {
            T results = default;
            Do(() => { results = callback(); });
            return results;
        }

        /// <summary>
        /// Wraps up calling the MongoDB to deal with common error scenarios..
        /// </summary>
        /// <param name="callback">Action to be performed.</param>
        protected virtual void Do(Action callback)
        {
            if (_disposed)
            {
#pragma warning disable DL0008
                throw new ObjectDisposedException("Attempt to use storage after it has been disposed.");
#pragma warning restore DL0008
            }

            try
            {
                callback();
            }
            catch (MongoConnectionException e)
            {
                throw new EventStoreUnavailable(e.Message, e);
            }
            catch (MongoException e)
            {
                throw new EventStorePersistenceError(e.Message, e);
            }
        }
    }
}