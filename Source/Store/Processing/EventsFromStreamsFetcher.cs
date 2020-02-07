// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Store.MongoDB.Streams;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IFetchEventsFromStreams" />.
    /// </summary>
    public class EventsFromStreamsFetcher : IFetchEventsFromStreams
    {
        readonly FilterDefinitionBuilder<StreamEvent> _streamEventFilter = Builders<StreamEvent>.Filter;
        readonly EventStoreConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsFromStreamsFetcher"/> class.
        /// </summary>
        /// <param name="connection">An <see cref="EventStoreConnection"/> to a MongoDB EventStore.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public EventsFromStreamsFetcher(EventStoreConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommittedEventWithPartition> Fetch(StreamId streamId, StreamPosition streamPosition)
        {
            var committedEventWithPartition = await _connection.StreamEvents.Find(
                _streamEventFilter.Eq(_ => _.StreamIdAndPosition, new StreamIdAndPosition(streamId, streamPosition)))
                .Project(_ => _.ToCommittedEventWithPartition(_.PartitionId))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (committedEventWithPartition == default) throw new NoEventInStreamAtPosition(streamId, streamPosition);
            return committedEventWithPartition;
        }

        /// <inheritdoc/>
        public async Task<StreamPosition> FindNext(StreamId streamId, PartitionId partitionId, StreamPosition fromPosition)
        {
            var streamEvent = await _connection.StreamEvents.Find(
                _streamEventFilter.Eq(_ => _.StreamIdAndPosition.StreamId, streamId.Value)
                & _streamEventFilter.Eq(_ => _.PartitionId, partitionId.Value)
                & _streamEventFilter.Gte(_ => _.StreamIdAndPosition.Position, fromPosition.Value))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (streamEvent == default) return uint.MaxValue;
            return streamEvent.StreamIdAndPosition.Position;
        }
    }
}