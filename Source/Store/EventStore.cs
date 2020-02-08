// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Artifacts;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Represents the MongoDB implementation of <see cref="IEventStore" />.
    /// </summary>
    public class EventStore : IEventStore
    {
        readonly EventStoreMongoDBConfiguration _config;
        readonly ILogger _logger;
        bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStore"/> class.
        /// </summary>
        /// <param name="config">A mongodb instance with associated configuration.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance to log significant events.</param>
        public EventStore(EventStoreMongoDBConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <inheritdoc />
        public CommittedEventStream Commit(UncommittedEventStream uncommittedEvents)
        {
            var commit = uncommittedEvents.AsBsonCommit();
            return Do(() =>
            {
                try
                {
                    var result = ExecuteCommit(commit);
                    if (result.IsSuccessfulCommit())
                    {
                        var sequence_number = result[Constants.ID].ToUlong();
                        return uncommittedEvents.ToCommitted(sequence_number);
                    }
                    else if (result.IsKnownError())
                    {
                        if (result.IsPreviousVersion())
                        {
                            throw new EventSourceConcurrencyConflict(result.ToEventSourceVersion(), uncommittedEvents.Source.Version);
                        }
                        else if (IsDuplicateCommit(uncommittedEvents.Id))
                        {
                            throw new CommitIsADuplicate();
                        }
                        else
                        {
                            throw new EventSourceConcurrencyConflict(result[Constants.ERROR].AsBsonDocument.ToEventSourceVersion(), uncommittedEvents.Source.Version);
                        }
                    }
                    else
                    {
                        throw new UnknownCommitError(result?.ToString() ?? "[NULL]");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Exception committing event stream");
                    throw;
                }
            });
        }

        /// <inheritdoc />
        public Commits Fetch(EventSourceKey eventSourceKey)
        {
            return FindCommitsWithSorting(eventSourceKey.ToFilter());
        }

        /// <inheritdoc />
        public Commits FetchFrom(EventSourceKey eventSourceKey, CommitVersion commitVersion)
        {
            return FindCommitsWithSorting(eventSourceKey.ToFilter() & commitVersion.ToFilter());
        }

        /// <inheritdoc />
        public Commits FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            return FindCommitsWithSorting(commit.ToFilter());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfType(ArtifactId artifactId)
        {
            var commits = FindCommitsWithSorting(artifactId.ToFilter());
            return GetEventsFromCommits(commits, artifactId);
        }

        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfTypeAfter(ArtifactId artifactId, CommitSequenceNumber commitSequenceNumber)
        {
            var commits = FindCommitsWithSorting(commitSequenceNumber.ToFilter() & artifactId.ToFilter());
            return GetEventsFromCommits(commits, artifactId);
        }

        /// <inheritdoc />
        public EventSourceVersion GetCurrentVersionFor(EventSourceKey eventSource)
        {
            var builder = Builders<BsonDocument>.Sort;
            var filter = eventSource.ToFilter();
            var sort = builder.Descending(VersionConstants.COMMIT);
            var version = _config.Commits.Find(filter).Sort(sort).Limit(1).FirstOrDefault();
            if (version == null)
                return EventSourceVersion.NoVersion;

            return version.ToEventSourceVersion();
        }

        /// <inheritdoc />
        public EventSourceVersion GetNextVersionFor(EventSourceKey eventSource)
        {
            return GetCurrentVersionFor(eventSource).NextCommit();
        }

        /// <summary>
        /// Helper function to perform an action and return the results.
        /// </summary>
        /// <param name="callback">Action to be performed.</param>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <returns>Instance of T.</returns>
        protected virtual T Do<T>(Func<T> callback)
        {
            T results = default;
            Do(() => { results = callback(); });
            return results;
        }

        /// <summary>
        /// Wraps up calling the MongoDB to deal with common error scenarios.
        /// </summary>
        /// <param name="callback">Action to be performed.</param>
        protected virtual void Do(Action callback)
        {
            if (_disposed)
            {
#pragma warning disable DL0008
                throw new ObjectDisposedException("Attempt to use storage after it has been disposed.");
#pragma warning restore DL0008
            }

            try
            {
                callback();
            }
            catch (MongoConnectionException e)
            {
                throw new EventStoreUnavailable(e.Message, e);
            }
            catch (MongoException e)
            {
                throw new EventStorePersistenceError(e.Message, e);
            }
        }

        bool IsDuplicateCommit(CommitId commit)
        {
            var doc = _config.Commits.Find(commit.ToFilter()).SingleOrDefault();
            return doc != null;
        }

        BsonDocument ExecuteCommit(BsonDocument commit)
        {
            var result = _config.Commits.Database.Eval(EventStoreMongoDBConfiguration.UpdateJSCommand, new BsonValue[] { commit });

            if (result == null)
                throw new InvalidCommitResponse(commit);

            return result.AsBsonDocument;
        }

        Commits FindCommitsWithSorting(FilterDefinition<BsonDocument> filter)
        {
            var builder = Builders<BsonDocument>.Sort;
            var sort = builder.Ascending(Constants.VERSION);
            var docs = _config.Commits.Find(filter).Sort(sort).ToList();
            var commits = new List<CommittedEventStream>();
            foreach (var doc in docs)
            {
                commits.Add(doc.ToCommittedEventStream());
            }

            return new Commits(commits);
        }

        SingleEventTypeEventStream GetEventsFromCommits(IEnumerable<CommittedEventStream> commits, ArtifactId eventType)
        {
            var events = new List<CommittedEventEnvelope>();
            foreach (var commit in commits)
            {
                events.AddRange(commit.Events.FilteredByEventType(eventType).Select(e => new CommittedEventEnvelope(commit.Sequence, e.Metadata, e.Event)));
            }

            return new SingleEventTypeEventStream(events);
        }
    }
}