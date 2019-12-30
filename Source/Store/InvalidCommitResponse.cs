// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Exception that gets thrown when we get an invalid response during commiting of an event.
    /// </summary>
    public class InvalidCommitResponse : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCommitResponse"/> class.
        /// </summary>
        /// <param name="document"><see cref="BsonDocument"/> that was attempted to be committted.</param>
        public InvalidCommitResponse(BsonDocument document)
            : base($"Invalid response when committing '{document}'.")
        {
        }
    }
}