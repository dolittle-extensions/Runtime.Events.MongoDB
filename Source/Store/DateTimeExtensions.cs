// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace EventStore.MongoDB
{
    /// <summary>
    /// Extends DateTime.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a nullable DateTime into its DateTimeOffset equivalent.
        /// </summary>
        /// <param name="date">The DateTime to convert.</param>
        /// <returns>A DateTimeOffset equivalent if it has a value, otherwise DateTimeOffset.MinValue.</returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime? date)
        {
            return date ?? DateTimeOffset.MinValue;
        }
    }
}