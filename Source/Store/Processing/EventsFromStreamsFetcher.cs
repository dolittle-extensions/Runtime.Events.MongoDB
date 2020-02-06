// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IFetchEventsFromStreams" />.
    /// </summary>
    public class EventsFromStreamsFetcher : IFetchEventsFromStreams
    {
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
        public Task<CommittedEventWithPartition> Fetch(StreamId streamId, StreamPosition streamPosition)
        {
            return Task.FromResult<CommittedEventWithPartition>(null);
        }

        /// <inheritdoc/>
        public Task<StreamPosition> FindNext(StreamId streamId, PartitionId partitionId, StreamPosition fromPosition)
        {
            return Task.FromResult(StreamPosition.Start);
        }
    }
}