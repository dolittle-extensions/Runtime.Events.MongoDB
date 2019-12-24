// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable CA1707, SA1310, CS1591, SA1600

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Constants related to the Event Metadata.
    /// </summary>
    public static class EventConstants
    {
        public const string ORIGINAL_CONTEXT = "original_context";
        public const string OCCURRED = "occurred";
        public const string EVENT = "event";
        public const string SHA = "SHA";
        public const string EVENT_ARTIFACT = "event_artifact";
        public const string APPLICATION = "application";
        public const string BOUNDED_CONTEXT = "bounded_context";
        public const string TENANT = "tenant";
        public const string ENVIRONMENT = "environment";
        public const string CLAIMS = "claims";
        public const string CLAIM_NAME = "value";
        public const string CLAIM_VALUE = "type";
        public const string CLAIM_VALUE_TYPE = "value_type";
    }
}
