// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Applications;
using Dolittle.Artifacts;
using Dolittle.Execution;
using Dolittle.Runtime.Events.Store.MongoDB.EventLog;
using Dolittle.Tenancy;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// A class capable of committing events.
    /// </summary>
    public class EventCommitter
    {
        readonly IClientSessionHandle _transaction;
        readonly IMongoCollection<Event> _eventLog;
        readonly Cause _cause;
        readonly CorrelationId _correlation;
        readonly Microservice _microservice;
        readonly TenantId _tenant;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCommitter"/> class.
        /// </summary>
        /// <param name="transaction">The <see cref="IClientSessionHandle"/> representing the MongoDB transaction encompassing this aggregate root version update.</param>
        /// <param name="eventLog">The <see cref="IMongoCollection{Event}"/> where events are stored in the event store.</param>
        /// <param name="cause">The <see cref="Cause"/> of the events.</param>
        /// <param name="correlation">The <see cref="CorrelationId"/> of the events.</param>
        /// <param name="microservice">The <see cref="Microservice"/> that produced the events.</param>
        /// <param name="tenant">The <see cref="TenantId"/> the events were produced in.</param>
        public EventCommitter(IClientSessionHandle transaction, IMongoCollection<Event> eventLog, Cause cause, CorrelationId correlation, Microservice microservice, TenantId tenant)
        {
            _transaction = transaction;
            _eventLog = eventLog;
            _cause = cause;
            _correlation = correlation;
            _microservice = microservice;
            _tenant = tenant;
        }

        /// <summary>
        /// Commits a single <see cref="UncommittedEvent"/> to the event log.
        /// </summary>
        /// <param name="version">The expected next <see cref="EventLogVersion"/> of the event log.</param>
        /// <param name="occurred">The <see cref="DateTimeOffset"/> when the event occured.</param>
        /// <param name="event">The <see cref="UncommittedEvent"/> to commit.</param>
        /// <param name="committedEvent">The <see cref="CommittedEvent"/> that has been written to the event log.</param>
        /// <returns>A value indicating whether the commit operation was successful.</returns>
        public bool TryCommitEvent(EventLogVersion version, DateTimeOffset occurred, UncommittedEvent @event, out CommittedEvent committedEvent)
        {
            if (InsertEvent(version, occurred, @event, new AggregateMetadata()))
            {
                committedEvent = new CommittedEvent(
                    version,
                    occurred,
                    _correlation,
                    _microservice,
                    _tenant,
                    _cause,
                    @event.Type,
                    @event.Content);
                return true;
            }
            else
            {
                committedEvent = null;
                return false;
            }
        }

        /// <summary>
        /// Commits a single <see cref="UncommittedEvent"/> applied to an event source by an aggregate root to the event log.
        /// </summary>
        /// <param name="eventSource">The <see cref="EventSourceId"/> the event was applied to.</param>
        /// <param name="aggregateRoot">The <see cref="Artifact"/> identifying the type of the aggregate root that applied the event.</param>
        /// <param name="aggregateRootVersion">The <see cref="AggregateRootVersion"/> of the aggregate root that applied the event.</param>
        /// <param name="version">The expected next <see cref="EventLogVersion"/> of the event log.</param>
        /// <param name="occured">The <see cref="DateTimeOffset"/> when the event occured.</param>
        /// <param name="event">The <see cref="UncommittedEvent"/> to commit.</param>
        /// <param name="committedEvent">The <see cref="CommittedAggregateEvent"/> that has been written to the event log.</param>
        /// <returns>A value indicating whether the commit operation was successful.</returns>
        public bool TryCommitAggregateEvent(EventSourceId eventSource, Artifact aggregateRoot, AggregateRootVersion aggregateRootVersion, EventLogVersion version, DateTimeOffset occured, UncommittedEvent @event, out CommittedAggregateEvent committedEvent)
        {
            if (InsertEvent(version, occured, @event, new AggregateMetadata
            {
                WasAppliedByAggregate = true,
                EventSourceId = eventSource,
                TypeId = aggregateRoot.Id,
                TypeGeneration = aggregateRoot.Generation,
                Version = aggregateRootVersion,
            }))
            {
                committedEvent = new CommittedAggregateEvent(
                    eventSource,
                    aggregateRoot,
                    aggregateRootVersion,
                    version,
                    occured,
                    _correlation,
                    _microservice,
                    _tenant,
                    _cause,
                    @event.Type,
                    @event.Content);
                return true;
            }
            else
            {
                committedEvent = null;
                return false;
            }
        }

        bool InsertEvent(EventLogVersion version, DateTimeOffset occured, UncommittedEvent @event, AggregateMetadata aggregate)
        {
            try
            {
                _eventLog.InsertOne(_transaction, new Event
                {
                    EventLogVersion = version,
                    Metadata = new EventMetadata
                    {
                        Occurred = occured,
                        Correlation = _correlation,
                        Microservice = _microservice,
                        Tenant = _tenant,
                        CauseType = _cause.Type,
                        CausePosition = _cause.Position,
                        TypeId = @event.Type.Id,
                        TypeGeneration = @event.Type.Generation,
                    },
                    Aggregate = aggregate,
                    Content = BsonDocument.Parse(@event.Content),
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
    }
}