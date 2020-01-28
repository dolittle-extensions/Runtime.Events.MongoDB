// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Artifacts;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store.MongoDB.Aggregates;
using MongoDB.Driver;

#pragma warning disable DL0008

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Represents the MongoDB implementation of <see cref="IEventStore"/>.
    /// </summary>
    public class EventStore : IEventStore
    {
        readonly EventStoreConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStore"/> class.
        /// </summary>
        /// <param name="connection">An <see cref="EventStoreConnection"/> to a MongoDB EventStore.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public EventStore(EventStoreConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public CommittedEvents CommitEvents(UncommittedEvents events)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public CommittedAggregateEvents CommitAggregateEvents(UncommittedAggregateEvents events)
        {
            try
            {
                using var session = _connection.MongoClient.StartSession();
                return session.WithTransaction((transaction, cancel) =>
                {
                    // Write much events
                    var nextVersion = events.ExpectedAggregateRootVersion + 2;

                    var committer = new AggregateVersionCommitter(
                        transaction,
                        _connection.Aggregates,
                        events.EventSource,
                        events.AggregateRoot.Id,
                        events.ExpectedAggregateRootVersion);

                    if (committer.TryIncrementVersionTo(nextVersion))
                    {
                        return new CommittedAggregateEvents(events.EventSource, events.AggregateRoot, events.ExpectedAggregateRootVersion, Array.Empty<CommittedAggregateEvent>());
                    }
                    else
                    {
                        throw new AggregateRootConcurrencyConflict(0, 0);
                    }
                });
            }
            catch (AggregateRootConcurrencyConflict)
            {
                var currentVersion = FetchAggregateRootVersion(events.EventSource, events.AggregateRoot.Id);
                throw new AggregateRootConcurrencyConflict(currentVersion, events.ExpectedAggregateRootVersion);
            }
        }

        /// <inheritdoc/>
        public CommittedAggregateEvents FetchForAggregate(EventSourceId eventSource, ArtifactId aggregateRoot)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        AggregateRootVersion FetchAggregateRootVersion(EventSourceId eventSource, ArtifactId aggregateRoot)
        {
            var filter = Builders<AggregateRoot>.Filter;

            var aggregates = _connection.Aggregates.Find(filter.Eq(_ => _.EventSource, eventSource.Value) & filter.Eq(_ => _.AggregateType, aggregateRoot.Value)).ToList();

            return aggregates.Count switch
            {
                0 => AggregateRootVersion.Initial,
                1 => aggregates[0].Version,
                _ => throw new MultipleAggregateInstancesFound(eventSource, aggregateRoot),
            };
        }
    }
}