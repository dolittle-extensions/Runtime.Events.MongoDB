/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.Relativity.MongoDB
{
    /// <summary>
    /// Extension methods for the <see cref="EventHorizonKey"/>
    /// </summary>
    public static class EventHorizonKeyExtensions
    {
        /// <summary>
        /// Transforms the <see cref="EventHorizonKey"/> and offset to <see cref="BsonDocument"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static BsonDocument ToOffsetBson(this EventHorizonKey key, ulong offset)
        {
            return new BsonDocument( new Dictionary<string,object>
            {
                { Constants.ID, key.AsId() },
                { Constants.OFFSET, offset }
            });
        }
        /// <summary>
        /// Gets the id representation of the <see cref="EventHorizonKey"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AsId(this EventHorizonKey key) 
        {
            return key.Application.Value.ToString() + "-" + key.BoundedContext.Value.ToString();
        }
    }
}