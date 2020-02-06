// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Artifacts;
using Dolittle.Runtime.Events.Store.MongoDB.Aggregates;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// A class capable of ensuring consistency with optimistic concurrency for aggregate root versions in the event store.
    /// </summary>
    public class AggregateVersionCommitter
    {
        readonly IClientSessionHandle _transaction;
        readonly IMongoCollection<AggregateRoot> _aggregates;
        readonly EventSourceId _eventSource;
        readonly ArtifactId _aggregateRoot;
        readonly AggregateRootVersion _expectedVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateVersionCommitter"/> class.
        /// </summary>
        /// <param name="transaction">The <see cref="IClientSessionHandle"/> representing the MongoDB transaction encompassing this aggregate root version update.</param>
        /// <param name="aggregates">The <see cref="IMongoCollection{AggregateRoot}"/> where aggregegate root instances are stored in the event store.</param>
        /// <param name="eventSource">The <see cref="EventSourceId"/> of the aggregate root instance identifier.</param>
        /// <param name="aggregateRoot">The <see cref="ArtifactId"/> of the aggregate root instance identifier.</param>
        /// <param name="expectedVersion">The expected <see cref="AggregateRootVersion"/> of the aggregate root instance in the event store.</param>
        public AggregateVersionCommitter(IClientSessionHandle transaction, IMongoCollection<AggregateRoot> aggregates, EventSourceId eventSource, ArtifactId aggregateRoot, AggregateRootVersion expectedVersion)
        {
            _transaction = transaction;
            _aggregates = aggregates;
            _eventSource = eventSource;
            _aggregateRoot = aggregateRoot;
            _expectedVersion = expectedVersion;
        }

        /// <summary>
        /// Tries to increment the version of the aggregate root instance in the event store.
        /// </summary>
        /// <param name="nextVersion">The new version of the aggregate root instance to persist.</param>
        /// <returns>A value indicating whether the version increment was successfull or not.</returns>
        public bool TryIncrementVersionTo(AggregateRootVersion nextVersion)
        {
            ThrowIfNextVersionIsNotGreaterThanExpectedVersion(nextVersion);

            if (_expectedVersion == AggregateRootVersion.Initial)
            {
                return WriteFirstAggregateRootDocument(nextVersion);
            }
            else
            {
                return UpdateExistingAggregateRootDocument(nextVersion);
            }
        }

        bool WriteFirstAggregateRootDocument(AggregateRootVersion nextVersion)
        {
            try
            {
                _aggregates.InsertOne(
                    _transaction,
                    new AggregateRoot
                    {
                        EventSource = _eventSource,
                        AggregateType = _aggregateRoot,
                        Version = nextVersion,
                    });
                return true;
            }
            catch (MongoDuplicateKeyException)
            {
                return false;
            }
            catch (MongoWriteException exception)
            {
                if (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return false;
                }

                throw;
            }
            catch (MongoBulkWriteException exception)
            {
                foreach (var error in exception.WriteErrors)
                {
                    if (error.Category == ServerErrorCategory.DuplicateKey)
                    {
                        return false;
                    }
                }

                throw;
            }
        }

        bool UpdateExistingAggregateRootDocument(AggregateRootVersion nextVersion)
        {
            var filter = Builders<AggregateRoot>.Filter;

            var result = _aggregates.UpdateOne(
                _transaction,
                filter.Eq(_ => _.EventSource, _eventSource.Value) & filter.Eq(_ => _.AggregateType, _aggregateRoot.Value) & filter.Eq(_ => _.Version, _expectedVersion.Value),
                Builders<AggregateRoot>.Update.Set(_ => _.Version, nextVersion.Value));

            return result.ModifiedCount == 1;
        }

        void ThrowIfNextVersionIsNotGreaterThanExpectedVersion(AggregateRootVersion nextVersion)
        {
            if (nextVersion <= _expectedVersion)
            {
                throw new NextAggregateRootVersionMustBeGreaterThanCurrentVersion(_expectedVersion, nextVersion);
            }
        }
    }
}