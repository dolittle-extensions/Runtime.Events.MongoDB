// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Store.MongoDB.Streams;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Exception that gets thrown when there was no <see cref="StreamEvent" /> at <see cref="StreamId" /> in <see cref="StreamPosition" />.
    /// </summary>
    public class NoEventInStreamAtPosition : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoEventInStreamAtPosition"/> class.
        /// </summary>
        /// <param name="streamId">The <see cref="StreamId" />.</param>
        /// <param name="streamPosition">The <see cref="StreamPosition" />.</param>
        public NoEventInStreamAtPosition(StreamId streamId, StreamPosition streamPosition)
            : base($"No event in stream '{streamId.Value} at position '{streamPosition.Value}'")
        {
        }
    }
}