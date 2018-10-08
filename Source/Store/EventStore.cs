/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Artifacts;
using Dolittle.Events;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// MongoDB implementation of <see cref="IEventStore" />
    /// </summary>
    public class EventStore : IEventStore
    {

        object lock_object = new object();
        readonly EventStoreMongoDBConfiguration _config;
        readonly ILogger _logger;

        /// <summary>
        /// Instantiates an instance of the EventStore
        /// </summary>
        /// <param name="config">A mongodb instance with associated configuration</param>
        /// <param name="logger">An <see cref="ILogger"/> instance to log significant events</param>
        public EventStore(EventStoreMongoDBConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <inheritdoc />
        public CommittedEventStream Commit(UncommittedEventStream uncommittedEvents)
        {
            var commit = uncommittedEvents.AsBsonCommit();
            return Do<CommittedEventStream>(() => {
                try
                {
                    var result = ExecuteCommit(commit);
                    if(result.IsSuccessfulCommit()){
                        var sequence_number =  result[Constants.ID].ToUlong();
                        var committed = uncommittedEvents.ToCommitted(sequence_number);
                        UpdateVersion(committed.Source);
                        return committed;
                    } else if(result.IsKnownError()){
                        ValidateVersionAgainstCommits(uncommittedEvents.Source.EventSource);
                        if(result.IsPreviousVersion()){
                            throw new EventSourceConcurrencyConflict($"Current Version is {result["version"]}, tried to commit {uncommittedEvents.Source.Version.Commit}");
                        }
                        else if(IsDuplicateCommit(uncommittedEvents.Id)){
                            throw new CommitIsADuplicate();
                        } else {
                            throw new EventSourceConcurrencyConflict();
                        }
                    } else {
                        throw new Exception("Unknown error type");
                    }
                } catch(Exception ex)
                {
                    _logger.Error(ex,"Exception committing event stream");
                    throw;
                }
            });
        }

        /// <inheritdoc />
        public Commits Fetch(EventSourceId eventSourceId)
        {
           return FindCommitsWithSorting(eventSourceId.ToFilter()); 
        }

        /// <inheritdoc />
        public Commits FetchFrom(EventSourceId eventSourceId, CommitVersion commitVersion)
        {
            return FindCommitsWithSorting(eventSourceId.ToFilter() & commitVersion.ToFilter());
        }

        /// <inheritdoc />
        public Commits FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            return FindCommitsWithSorting(commit.ToFilter());
        }

        void ValidateVersionAgainstCommits(EventSourceId eventSource)
        {
            var versionFromCommits = GetVersionFromCommits(eventSource);
            var versionFromVersion = GetCurrentVersionFor(eventSource);
            if(versionFromCommits.Version.CompareTo(versionFromVersion) > 0)
            {
                UpdateVersion(versionFromCommits);
            }
        }
        VersionedEventSource GetVersionFromCommits(EventSourceId eventSource)
        {
            var builder = Builders<BsonDocument>.Sort;
            var sort = builder.Ascending(Constants.VERSION);
            var commitDoc = _config.Commits.Find(eventSource.ToFilter()).Sort(sort).Limit(1).ToList().SingleOrDefault();
            if(commitDoc == null)
                return new VersionedEventSource(EventSourceVersion.NoVersion,eventSource,Guid.Empty);

            return commitDoc.ToCommittedEventStream().Source;
        }
        bool IsDuplicateCommit(CommitId commit)
        {
            var doc = _config.Commits.Find(commit.ToFilter()).SingleOrDefault();
            return doc != null;
        }
        BsonDocument ExecuteCommit(BsonDocument commit)
        {
            var result = _config.Commits.Database.Eval(EventStoreMongoDBConfiguration.UpdateJSCommand, new BsonValue[]{ commit });
            
            if(result == null)
                throw new Exception("The error response is not in the format of a Bson Document.  Cannot process."); //use custom exception
            return result.AsBsonDocument;
        }

        #region IDisposable Support
        /// <summary>
        /// Disposed flag to detect redundant calls
        /// </summary>
        protected bool disposedValue = false; 

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Disposes of the EventStore
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Helper function to perform an action and return the results
        /// </summary>
        /// <param name="callback">Action to be performed</param>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <returns>Instance of T</returns>
        protected virtual T Do<T>(Func<T> callback)
        {
            T results = default(T);
            Do(() => { results = callback(); });
            return results;
        }

        /// <summary>
        /// Wraps up calling the MongoDB to deal with common error scenarios.
        /// </summary>
        /// <param name="callback"></param>
        protected virtual void Do(Action callback)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("Attempt to use storage after it has been disposed.");
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
        public EventSourceVersion GetCurrentVersionFor(EventSourceId eventSource)
        {
            var version = _config.Versions.Find(eventSource.ToFilter()).SingleOrDefault();
            if(version == null)
                return EventSourceVersion.NoVersion;
            
            return version.ToEventSourceVersion();
        }

        /// <inheritdoc />
        public EventSourceVersion GetNextVersionFor(EventSourceId eventSource)
        {
            return GetCurrentVersionFor(eventSource).NextCommit();
        }

        Commits FindCommitsWithSorting(FilterDefinition<BsonDocument> filter)
        {
            var builder = Builders<BsonDocument>.Sort;
            var sort = builder.Ascending(Constants.VERSION);
            var docs = _config.Commits.Find(filter).Sort(sort).ToList();
            var commits = new List<CommittedEventStream>();
            foreach(var doc in docs)
            {
                commits.Add(doc.ToCommittedEventStream());
            } 
            return new Commits(commits);
        } 


        SingleEventTypeEventStream GetEventsFromCommits(IEnumerable<CommittedEventStream> commits, ArtifactId eventType)
        {
            var events = new List<CommittedEventEnvelope>();
            foreach(var commit in commits)
            {
                events.AddRange(commit.Events.FilteredByEventType(eventType).Select(e => new CommittedEventEnvelope(commit.Sequence,e.Metadata,e.Event)));
            }
            return new SingleEventTypeEventStream(events);
        }

        /// <summary>
        /// Updates the Version of the <see cref="EventSource" />
        /// </summary>
        /// <param name="version">The version to update to</param>
        protected void UpdateVersion(VersionedEventSource version)
        {
            if (version != null)
            {
                try
                {
                    var versionBson = version.AsBsonVersion();
                    versionBson.Add(Constants.ID, version.EventSource.Value);
                    var result = _config.Versions.ReplaceOne(version.EventSource.ToFilter(),versionBson,new UpdateOptions { IsUpsert = true });
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    //Swallowing the exception for now but this could cause a problem for our "stateless" 
                    _logger.Error(ex,"Exception updating version collection to latest version");
                }
            }
        }
    }
}