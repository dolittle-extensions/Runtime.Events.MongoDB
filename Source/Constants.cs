// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Store;

#pragma warning disable CA1707, SA1310, CS1591, SA1600

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Generic Constants for use within the <see cref="IEventStore" />.
    /// </summary>
    public static class Constants
    {
        public const string EVENTSOURCE_ID = "eventsource_id";
        public const string VERSION = "version";
        public const string MAJOR_VERSION = "major";
        public const string MINOR_VERSION = "minor";
        public const string REVISION = "revision";
        public const string GENERATION = "generation";
        public const string EVENT_SOURCE_ARTIFACT = "event_source_artifact";
        public const string ID = "_id";
        public const string CORRELATION_ID = "correlation_id";
        public const string ERROR = "err";
        public const string QUERY_EVENT_ARTIFACT = "events.event_artifact";
        public const string OFFSET = "offset";
    }
}
