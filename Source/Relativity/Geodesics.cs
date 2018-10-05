/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

namespace Dolittle.Runtime.Events.Relativity.MongoDB
{
    using Dolittle.Logging;
    using Dolittle.Runtime.Events.MongoDB;
    using Dolittle.Runtime.Events.Processing;
    using Dolittle.Runtime.Events.Store;
    using global::MongoDB.Bson;
    using global::MongoDB.Driver;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// A MongoDB implementation of <see cref="IGeodesics" />
    /// </summary>
    public class Geodesics : IGeodesics
    {
        object lock_object = new object();
        /// <summary>
        /// Name of the Offsets collection
        /// </summary>
        public const string OFFSETS = "geodesic_offsets"; 

        private IMongoDatabase _database;
        private MongoCollectionSettings _offsetsSettings;
        private ILogger _logger;

        /// <summary>
        /// Instantiates an instance of <see cref="IGeodesics" />
        /// </summary>
        /// <param name="database">A MongoDB instance</param>
        /// <param name="logger">A logger instance</param>
        public Geodesics(IMongoDatabase database, ILogger logger)
        {
            _database = database;
            _logger = logger;
            Bootstrap();
        }

        void Bootstrap()
        {
            _offsetsSettings = new MongoCollectionSettings{ AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
        }

        private IMongoCollection<BsonDocument> Offsets => _database.GetCollection<BsonDocument>(OFFSETS, _offsetsSettings);

        /// <inheritdoc />
        public ulong GetOffset(EventHorizonKey key)
        {
            var offset = Offsets.Find(key.ToFilter()).SingleOrDefault();
            if(offset == null)
                return 0;
            
            return offset[Constants.OFFSET].ToUlong();
        }

        /// <inheritdoc />
        public void SetOffset(EventHorizonKey key, ulong offset)
        {
            var offsetBson = new BsonDocument( new Dictionary<string,object>
            {
                { Constants.ID, key.GetHashCode() },
                { Constants.OFFSET, offset }
            });
            
            Offsets.ReplaceOne(key.ToFilter(),offsetBson,new UpdateOptions { IsUpsert = true });
        }

        #region IDisposable Support
        /// <summary>
        /// Disposed flag to detect redundant calls
        /// </summary>
        protected bool disposedValue = false; 

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Disposes of the EventStore
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Helper function to perform an action and return the results
        /// </summary>
        /// <param name="callback">Action to be performed</param>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <returns>Instance of T</returns>
        protected virtual T Do<T>(Func<T> callback)
        {
            T results = default(T);
            Do(() => { results = callback(); });
            return results;
        }

        /// <summary>
        /// Wraps up calling the MongoDB to deal with common error scenarios.
        /// </summary>
        /// <param name="callback"></param>
        protected virtual void Do(Action callback)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("Attempt to use storage after it has been disposed.");
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