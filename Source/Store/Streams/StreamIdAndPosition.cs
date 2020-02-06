// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dolittle.Runtime.Events.Store.MongoDB.Streams
{
    /// <summary>
    /// Represents the stream id and stream position of a <see cref="StreamEvent" />.
    /// </summary>
    public class StreamIdAndPosition
    {
        /// <summary>
        /// Gets or sets the stream id.
        /// </summary>
        public Guid StreamId { get; set; }

        /// <summary>
        /// Gets or sets stream position.
        /// </summary>
        public uint Position { get; set; }
    }
}