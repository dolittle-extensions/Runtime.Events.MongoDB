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
        /// Constructs a <see cref="CommittedEventSteam" /> from the <see cref="UncomittedEventSteam" /> and the <see cref="CommitSequenceNumber" /> 
        /// </summary>
        /// <param name="uncommitted">the <see cref="UncomittedEventSteam" /></param>
        /// <param name="sequenceNumber">the <see cref="CommitSequenceNumber" /></param>
        /// <returns>The corresponding <see cref="CommittedEventSteam" /></returns>
        public static CommittedEventStream ToCommitted(this UncommittedEventStream uncommitted, CommitSequenceNumber sequenceNumber)
        {
            return new CommittedEventStream(sequenceNumber, uncommitted.Source, uncommitted.Id, uncommitted.CorrelationId, uncommitted.Timestamp, uncommitted.Events);
        }
    }
}