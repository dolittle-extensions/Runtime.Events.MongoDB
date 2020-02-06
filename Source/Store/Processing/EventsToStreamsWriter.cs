// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IWriteEventsToStreams" />.
    /// </summary>
    public class EventsToStreamsWriter : IWriteEventsToStreams
    {
        readonly EventStoreConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsToStreamsWriter"/> class.
        /// </summary>
        /// <param name="connection">An <see cref="EventStoreConnection"/> to a MongoDB EventStore.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public EventsToStreamsWriter(EventStoreConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<bool> Write(CommittedEvent @event, StreamId streamId, PartitionId partitionId)
        {
            return Task.FromResult(true);
        }
    }
}