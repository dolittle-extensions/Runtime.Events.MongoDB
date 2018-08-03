using System;
using System.Linq;
using Dolittle.Runtime.Events.Store;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Extends the <see cref="UncommittedEventStream" /> class
    /// </summary>
    public static class UncommittedEventStreamExtensions
    {

        /// <summary>
        /// Constructs a <see cref="CommittedEventStream" /> from the <see cref="UncommittedEventStream" /> and the <see cref="CommitSequenceNumber" /> 
        /// </summary>
        /// <param name="uncommitted">the <see cref="UncommittedEventStream" /></param>
        /// <param name="sequenceNumber">the <see cref="CommitSequenceNumber" /></param>
        /// <returns>The corresponding <see cref="CommittedEventStream" /></returns>
        public static CommittedEventStream ToCommitted(this UncommittedEventStream uncommitted, CommitSequenceNumber sequenceNumber)
        {
            return new CommittedEventStream(sequenceNumber, uncommitted.Source, uncommitted.Id, uncommitted.CorrelationId, uncommitted.Timestamp, uncommitted.Events);
        }
    }
}