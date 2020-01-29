// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Artifacts;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store.MongoDB.Aggregates;
using Dolittle.Runtime.Events.Store.MongoDB.EventLog;
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
                return session.WithTransaction(
                (transaction, cancel) =>
                {
                    var eventLogVersion = (uint)_connection.EventLog.CountDocuments(transaction, Builders<Event>.Filter.Empty);
                    var aggregateRootVersion = events.ExpectedAggregateRootVersion.Value;

                    var committedEvents = new List<CommittedAggregateEvent>();
                    var eventCommitter = new EventCommitter(transaction, _connection.EventLog, new Cause(CauseType.Command, 0), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

                    foreach (var @event in events)
                    {
                        if (eventCommitter.CommitAggregateEvent(events.EventSource, events.AggregateRoot, aggregateRootVersion, eventLogVersion, DateTimeOffset.Now, @event, out var committedEvent))
                        {
                            committedEvents.Add(committedEvent);
                            eventLogVersion++;
                            aggregateRootVersion++;
                        }
                        else
                        {
                            throw new EventLogDuplicateKeyError(eventLogVersion);
                        }
                    }

                    var committer = new AggregateVersionCommitter(
                        transaction,
                        _connection.Aggregates,
                        events.EventSource,
                        events.AggregateRoot.Id,
                        events.ExpectedAggregateRootVersion);

                    if (committer.TryIncrementVersionTo(aggregateRootVersion))
                    {
                        return new CommittedAggregateEvents(events.EventSource, events.AggregateRoot, events.ExpectedAggregateRootVersion, committedEvents);
                    }
                    else
                    {
                        throw new AggregateRootConcurrencyConflict(0, 0);
                    }
                }, new TransactionOptions(maxCommitTime: TimeSpan.FromSeconds(5)));
            }
            catch (AggregateRootConcurrencyConflict)
            {
                var currentVersion = FetchAggregateRootVersion(events.EventSource, events.AggregateRoot.Id);
                throw new AggregateRootConcurrencyConflict(currentVersion, events.ExpectedAggregateRootVersion);
            }
            catch (Exception ex)
            {
                throw new EventStorePersistenceError("Error persisting event to MongoDB event store", ex);
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