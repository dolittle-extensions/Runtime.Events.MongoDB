// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Runtime.Events.Store.MongoDB.EventLog;
using MongoDB.Bson.Serialization.Attributes;

namespace Dolittle.Runtime.Events.Store.MongoDB.Streams
{
    /// <summary>
    /// Represents an event stored in a stream.
    /// </summary>
    public class StreamEvent
    {
        /// <summary>
        /// Gets or sets the stream id and position.
        /// </summary>
        [BsonId]
        public StreamIdAndPosition StreamIdAndPosition { get; set; }

        /// <summary>
        /// Gets or sets the partition id.
        /// </summary>
        public Guid PartitionId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventMetadata"/> containing the platform generated event information.
        /// </summary>
        public EventMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the event sourcing specific <see cref="AggregateMetadata"/>.
        /// </summary>
        public AggregateMetadata Aggregate { get; set; }

        /// <summary>
        /// Gets or sets the domain specific event data.
        /// </summary>
        public string Content { get; set; }
    }
}