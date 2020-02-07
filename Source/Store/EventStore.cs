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
        readonly FilterDefinitionBuilder<Event> _eventFilter = Builders<Event>.Filter;
        readonly FilterDefinitionBuilder<AggregateRoot> _aggregateFilter = Builders<AggregateRoot>.Filter;
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
            try
            {
                using var session = _connection.MongoClient.StartSession();
                return session.WithTransaction((transaction, cancel) =>
                {
                    var eventLogVersion = (uint)_connection.EventLog.CountDocuments(transaction, Builders<Event>.Filter.Empty);

                    var committedEvents = new List<CommittedEvent>();
                    var eventCommitter = new EventCommitter(transaction, _connection.EventLog, new Cause(CauseType.Command, 0), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

                    foreach (var @event in events)
                    {
                        if (eventCommitter.TryCommitEvent(eventLogVersion, DateTimeOffset.Now, @event, out var committedEvent))
                        {
                            committedEvents.Add(committedEvent);
                            eventLogVersion++;
                        }
                        else
                        {
                            throw new EventLogDuplicateKeyError(eventLogVersion);
                        }
                    }

                    return new CommittedEvents(committedEvents);
                });
            }
            catch (Exception ex)
            {
                throw new EventStorePersistenceError("Error persisting event to MongoDB event store", ex);
            }
        }

        /// <inheritdoc/>
        public CommittedAggregateEvents CommitAggregateEvents(UncommittedAggregateEvents events)
        {
            try
            {
                using var session = _connection.MongoClient.StartSession();
                return session.WithTransaction((transaction, cancel) =>
                {
                    var eventLogVersion = (uint)_connection.EventLog.CountDocuments(transaction, _eventFilter.Empty);
                    var aggregateRootVersion = events.ExpectedAggregateRootVersion.Value;

                    var committedEvents = new List<CommittedAggregateEvent>();

                    // TODO: add execution context stuff...
                    var eventCommitter = new EventCommitter(
                        transaction,
                        _connection.EventLog,
                        new Cause(CauseType.Command, 0),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid());

                    foreach (var @event in events)
                    {
                        if (eventCommitter.TryCommitAggregateEvent(events.EventSource, events.AggregateRoot, aggregateRootVersion, eventLogVersion, DateTimeOffset.Now, @event, out var committedEvent))
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
                        return new CommittedAggregateEvents(events.EventSource, events.AggregateRoot.Id, events.ExpectedAggregateRootVersion, committedEvents);
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
            catch (Exception ex)
            {
                throw new EventStorePersistenceError("Error persisting event to MongoDB event store", ex);
            }
        }

        /// <inheritdoc/>
        public CommittedAggregateEvents FetchForAggregate(EventSourceId eventSource, ArtifactId aggregateRoot)
        {
            var version = FetchAggregateRootVersion(eventSource, aggregateRoot);
            if (version > AggregateRootVersion.Initial)
            {
                var events = _connection.EventLog
                    .Find(_eventFilter.Eq(_ => _.Aggregate.WasAppliedByAggregate, true)
                        & _eventFilter.Eq(_ => _.Aggregate.EventSourceId, eventSource.Value)
                        & _eventFilter.Eq(_ => _.Aggregate.TypeId, aggregateRoot.Value)
                        & _eventFilter.Lte(_ => _.Aggregate.Version, version.Value))
                    .Sort(Builders<Event>.Sort.Ascending(_ => _.Aggregate.Version))
                    .Project(_ => _.ToCommittedAggregateEvent())
                    .ToList();

                return new CommittedAggregateEvents(eventSource, aggregateRoot, version, events);
            }
            else
            {
                return new CommittedAggregateEvents(eventSource, aggregateRoot, AggregateRootVersion.Initial, Array.Empty<CommittedAggregateEvent>());
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        AggregateRootVersion FetchAggregateRootVersion(EventSourceId eventSource, ArtifactId aggregateRoot)
        {
            var aggregates = _connection.Aggregates.Find(_aggregateFilter.Eq(_ => _.EventSource, eventSource.Value) & _aggregateFilter.Eq(_ => _.AggregateType, aggregateRoot.Value)).ToList();

            return aggregates.Count switch
            {
                0 => AggregateRootVersion.Initial,
                1 => aggregates[0].Version,
                _ => throw new MultipleAggregateInstancesFound(eventSource, aggregateRoot),
            };
        }
    }
}