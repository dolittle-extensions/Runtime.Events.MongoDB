// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Store.MongoDB.EventLog;

namespace Dolittle.Runtime.Events.Store.MongoDB.Streams
{
    /// <summary>
    /// Extension methods for <see cref="StreamEvent" />.
    /// </summary>
    public static class StreamEventExtensions
    {
        /// <summary>
        /// Converts a <see cref="CommittedEvent" /> to a <see cref="StreamEvent" />.
        /// </summary>
        /// <param name="committedEvent">The <see cref="CommittedEvent" /> to convert.</param>
        /// <param name="streamId">The <see cref="StreamId" />.</param>
        /// <param name="streamPosition">The <see cref="StreamPosition" />.</param>
        /// <param name="partitionId">The <see cref="PartitionId" />.</param>
        /// <returns>The converted <see cref="StreamEvent" />.</returns>
        public static StreamEvent ToStreamEvent(this CommittedEvent committedEvent, StreamId streamId, StreamPosition streamPosition, PartitionId partitionId) =>
            new StreamEvent
            {
                StreamIdAndPosition = new StreamIdAndPosition { Position = streamPosition, StreamId = streamId.Value },
                Content = committedEvent.Content,
                Metadata = committedEvent.GetEventMetadata(),
                Aggregate = committedEvent.GetAggregateMetadata(),
                PartitionId = partitionId
            };
    }
}