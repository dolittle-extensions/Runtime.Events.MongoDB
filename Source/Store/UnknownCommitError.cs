// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Exception that gets thrown when we have an unknown error occur during commit.
    /// </summary>
    public class UnknownCommitError : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownCommitError"/> class.
        /// </summary>
        /// <param name="result">Result information.</param>
        public UnknownCommitError(string result)
            : base($"Unknown error during commit: {result}")
        {
        }
    }
}