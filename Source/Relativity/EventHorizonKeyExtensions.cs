// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.Relativity.MongoDB
{
    /// <summary>
    /// Extension methods for the <see cref="EventHorizonKey"/>.
    /// </summary>
    public static class EventHorizonKeyExtensions
    {
        /// <summary>
        /// Transforms the <see cref="EventHorizonKey"/> and offset to <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="key"><see cref="EventHorizonKey"/> to work with.</param>
        /// <param name="offset">Offset to use.</param>
        /// <returns>A new <see cref="BsonDocument"/> with the details.</returns>
        public static BsonDocument ToOffsetBson(this EventHorizonKey key, ulong offset)
        {
            return new BsonDocument(new Dictionary<string, object>
            {
                { Constants.ID, key.AsId() },
                { Constants.OFFSET, offset }
            });
        }

        /// <summary>
        /// Gets the id representation of the <see cref="EventHorizonKey"/>.
        /// </summary>
        /// <param name="key"><see cref="EventHorizonKey"/> to work with.</param>
        /// <returns>The identifier for the key.</returns>
        public static string AsId(this EventHorizonKey key)
        {
            return key.Application.Value.ToString() + "-" + key.BoundedContext.Value.ToString();
        }
    }
}