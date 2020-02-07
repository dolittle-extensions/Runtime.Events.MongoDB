// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IStreamProcessorStateRepository" />.
    /// </summary>
    public class StreamProcessorStateRepository : IStreamProcessorStateRepository
    {
        readonly FilterDefinitionBuilder<StreamProcessorState> _streamProcessorFilter = Builders<StreamProcessorState>.Filter;
        readonly UpdateDefinitionBuilder<StreamProcessorState> _streamProcessorUpdate = Builders<StreamProcessorState>.Update;
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
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public async Task<Events.Processing.StreamProcessorState> GetOrAddNew(Events.Processing.StreamProcessorId streamProcessorId)
        {
            try
            {
                var states = _connection.StreamProcessorStates;
                var state = await states.Find(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                if (state == default) state = StreamProcessorState.NewFromId(streamProcessorId);

                await states.InsertOneAsync(state).ConfigureAwait(false);

                return state.ToRuntimeRepresentation();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while getting or adding a new stream processor with id '{streamProcessorId} - Error {ex}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Events.Processing.StreamProcessorState> IncrementPosition(Events.Processing.StreamProcessorId streamProcessorId)
        {
            try
            {
                var states = _connection.StreamProcessorStates;
                var state = await states.Find(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                if (state == default) throw new StreamProcessorNotFound(streamProcessorId);

                state.Position++;

                var replaceResult = await states.ReplaceOneAsync(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)),
                    state)
                    .ConfigureAwait(false);

                if (replaceResult.MatchedCount == 0) throw new StreamProcessorNotFound(streamProcessorId);

                return state.ToRuntimeRepresentation();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while incrementing position of stream processor with id '{streamProcessorId}' - Error {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Events.Processing.StreamProcessorState> AddFailingPartition(Events.Processing.StreamProcessorId streamProcessorId, PartitionId partitionId, StreamPosition position, DateTimeOffset retryTime)
        {
            try
            {
                var states = _connection.StreamProcessorStates;
                var state = await states.Find(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                if (state == default) throw new StreamProcessorNotFound(streamProcessorId);

                if (state.FailingPartitions.ContainsKey(partitionId)) throw new FailingPartitionAlreadyExists(streamProcessorId, partitionId);

                state = await states.FindOneAndUpdateAsync(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)),
                    _streamProcessorUpdate.AddToSet(_ => _.FailingPartitions, new KeyValuePair<Guid, FailingPartitionState>(partitionId, new FailingPartitionState(position, retryTime))),
                    new FindOneAndUpdateOptions<StreamProcessorState, StreamProcessorState> { ReturnDocument = ReturnDocument.After })
                    .ConfigureAwait(false);

                return state.ToRuntimeRepresentation();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while adding failing partition '{partitionId}' in stream processor with id '{streamProcessorId}' - Error {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Events.Processing.StreamProcessorState> RemoveFailingPartition(Events.Processing.StreamProcessorId streamProcessorId, PartitionId partitionId)
        {
            try
            {
                var states = _connection.StreamProcessorStates;
                var state = await states.Find(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                if (state == default) throw new StreamProcessorNotFound(streamProcessorId);

                if (!state.FailingPartitions.ContainsKey(partitionId)) throw new FailingPartitionDoesNotExist(streamProcessorId, partitionId);

                state.FailingPartitions.Remove(partitionId);
                var replaceResult = await states.ReplaceOneAsync(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)),
                    state)
                    .ConfigureAwait(false);

                if (replaceResult.MatchedCount == 0) throw new FailingPartitionDoesNotExist(streamProcessorId, partitionId);

                return state.ToRuntimeRepresentation();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while removing failing partition '{partitionId}' in stream processor with id '{streamProcessorId}' - Error {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Events.Processing.StreamProcessorState> SetFailingPartitionState(Events.Processing.StreamProcessorId streamProcessorId, PartitionId partitionId, Events.Processing.FailingPartitionState failingPartitionState)
        {
            try
            {
                var states = _connection.StreamProcessorStates;
                var state = await states.Find(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)))
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                if (state == default) throw new StreamProcessorNotFound(streamProcessorId);

                if (!state.FailingPartitions.ContainsKey(partitionId)) throw new FailingPartitionDoesNotExist(streamProcessorId, partitionId);

                state.FailingPartitions[partitionId] = new FailingPartitionState(failingPartitionState.Position, failingPartitionState.RetryTime);

                var replaceResult = await states.ReplaceOneAsync(
                    _streamProcessorFilter.Eq(_ => _.Id, new StreamProcessorId(streamProcessorId.EventProcessorId, streamProcessorId.SourceStreamId)),
                    state)
                    .ConfigureAwait(false);

                if (replaceResult.MatchedCount == 0) throw new FailingPartitionDoesNotExist(streamProcessorId, partitionId);

                return state.ToRuntimeRepresentation();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while setting state of failing partition '{partitionId}' in stream processor with id '{streamProcessorId}' - Error {ex}");
                throw;
            }
        }
    }
}