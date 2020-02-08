// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using Dolittle.Runtime.Events.Store;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Relativity.MongoDB
{
    /// <summary>
    /// A MongoDB implementation of <see cref="IGeodesics" />.
    /// </summary>
    public class Geodesics : IGeodesics
    {
        const string OFFSETS = "geodesic_offsets";
        readonly IMongoDatabase _database;
        readonly MongoCollectionSettings _offsetsSettings;
        readonly IMongoCollection<BsonDocument> _offsets;
        readonly ILogger _logger;
        bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Geodesics"/> class.
        /// </summary>
        /// <param name="connection">The connection for the <see cref="IMongoDatabase"/>.</param>
        /// <param name="logger">A logger instance.</param>
        public Geodesics(Connection connection, ILogger logger)
        {
            _database = connection.Database;
            _logger = logger;
            _offsetsSettings = new MongoCollectionSettings { AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
            _offsets = _database.GetCollection<BsonDocument>(OFFSETS, _offsetsSettings);
        }

        /// <inheritdoc />
        public ulong GetOffset(EventHorizonKey key)
        {
            var offset = _offsets.Find(key.ToFilter()).SingleOrDefault();
            if (offset == null)
                return 0;

            return offset[Constants.OFFSET].ToUlong();
        }

        /// <inheritdoc />
        public void SetOffset(EventHorizonKey key, ulong offset)
        {
            var offsetBson = key.ToOffsetBson(offset);

            _offsets.ReplaceOne(key.ToFilter(), offsetBson, new UpdateOptions { IsUpsert = true });
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
            T results = default(T);
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