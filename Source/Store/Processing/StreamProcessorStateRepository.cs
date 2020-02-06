// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IStreamProcessorStateRepository" />.
    /// </summary>
    public class StreamProcessorStateRepository : IStreamProcessorStateRepository
    {
        readonly EventStoreConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamProcessorStateRepository"/> class.
        /// </summary>
        /// <param name="connection">An <see cref="EventStoreConnection"/> to a MongoDB EventStore.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public StreamProcessorStateRepository(EventStoreConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<StreamProcessorState> AddFailingPartition(StreamProcessorId streamProcessorId, PartitionId partitionId, StreamPosition position, DateTimeOffset retryTime)
        {
            return Task.FromResult<StreamProcessorState>(null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public Task<StreamProcessorState> GetOrAddNew(StreamProcessorId streamProcessorId)
        {
            return Task.FromResult<StreamProcessorState>(null);
        }

        /// <inheritdoc/>
        public Task<StreamProcessorState> IncrementPosition(StreamProcessorId streamProcessorId)
        {
            return Task.FromResult<StreamProcessorState>(null);
        }

        /// <inheritdoc/>
        public Task<StreamProcessorState> RemoveFailingPartition(StreamProcessorId streamProcessorId, PartitionId partitionId)
        {
            return Task.FromResult<StreamProcessorState>(null);
        }

        /// <inheritdoc/>
        public Task<StreamProcessorState> SetFailingPartitionState(StreamProcessorId streamProcessorId, PartitionId partitionId, FailingPartitionState failingPartitionState)
        {
            return Task.FromResult<StreamProcessorState>(null);
        }
    }
}