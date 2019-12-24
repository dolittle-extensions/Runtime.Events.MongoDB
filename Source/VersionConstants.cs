// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable CA1707, SA1310, CS1591, SA1600

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Constants related to the Version of the Event.
    /// </summary>
    public static class VersionConstants
    {
        public const string COMMIT = "commit";
        public const string SEQUENCE = "sequence";
        public const string SNAPSHOT = "shapshot";
        public const string EVENT_COUNT = "total_events";
    }
}
