using Dolittle.Runtime.Events.Store;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Dolittle.Artifacts;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// MongoDB implementation of <see cref="IEventStore" />
    /// </summary>
    public class EventStore : IEventStore
    {
        object lock_object = new object();
        /// <summary>
        /// Name of the Commits collection
        /// </summary>
        public const string COMMITS = "commits"; 
        /// <summary>
        /// Name of the Versions collection
        /// </summary>
        public const string VERSIONS = "versions";
        /// <summary>
        /// Name of the Snapshots collection
        /// </summary>
        public const string SNAPSHOTS = "shapshots";

        private IMongoDatabase _database;
        private MongoCollectionSettings _commitSettings;
        private MongoCollectionSettings _versionSettings;
        private MongoCollectionSettings _snapshotSettings;

        private string _updateJSCommand ="function (x){ return insert_commit(x);}";

        /// <summary>
        /// Instantiates an instance of the EventStore
        /// </summary>
        /// <param name="database">The mongodb instance that has the Event Store</param>
        public EventStore(IMongoDatabase database)
        {
            _database = database;
            Bootstrap();
        }

        void Bootstrap()
        {
            //BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document));
            _commitSettings = new MongoCollectionSettings{ AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
            _versionSettings = new MongoCollectionSettings{ AssignIdOnInsert = false, WriteConcern = WriteConcern.Unacknowledged };
            _snapshotSettings = new MongoCollectionSettings{ AssignIdOnInsert = false, WriteConcern = WriteConcern.Unacknowledged };
            CreateIndexes();
            CreateUpdateScript();
        }

        private void CreateUpdateScript()
        {
            var sys_functions = _database.GetCollection<BsonDocument>("system.js");
            var code = CommitConstants.INSERT_COMMIT;
            var commit_function_doc = new BsonDocument("value", new BsonJavaScript(code));
            sys_functions.UpdateOne(Builders<BsonDocument>.Filter.Eq(Constants.ID,"insert_commit"),  
                                        Builders<BsonDocument>.Update.Set("value", new BsonJavaScript(code)), 
                                        new UpdateOptions{ IsUpsert = true });
        }

        private void CreateIndexesForCommits()
        {
            var keys = Builders<BsonDocument>.IndexKeys.Descending(CommitConstants.COMMIT_ID);
            var model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument>{ Unique = true });
            Commits.Indexes.CreateOne(model);

            keys = Builders<BsonDocument>.IndexKeys.Ascending(Constants.EVENTSOURCE_ID).Descending(VersionConstants.COMMIT);
            model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument>{ Unique = true });
            Commits.Indexes.CreateOne(model);
        }

        private void CreateIndexes()
        {
            CreateIndexesForCommits();
        }

        private IMongoCollection<BsonDocument> Commits => _database.GetCollection<BsonDocument>(COMMITS, _commitSettings);

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
                        return uncommittedEvents.ToCommitted(sequence_number);
                    } else if(result.IsKnownError()){
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
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            });
        }

        private bool IsDuplicateCommit(CommitId commit)
        {
            var doc = Commits.Find(commit.ToFilter()).SingleOrDefault();
            return doc != null;
        }

        BsonDocument ExecuteCommit(BsonDocument commit)
        {
            var result = Commits.Database.Eval(_updateJSCommand, new BsonValue[]{ commit });
            
            if(result == null)
                throw new Exception("The error response is not in the format of a Bson Document.  Cannot process."); //use custom exception
            return result.AsBsonDocument;
        }

        /// <inheritdoc />
        public CommittedEvents Fetch(EventSourceId eventSourceId)
        {
           return FindCommitsWithSorting(eventSourceId.ToFilter()); 
        }

        /// <inheritdoc />
        public CommittedEvents FetchFrom(EventSourceId eventSourceId, CommitVersion commitVersion)
        {
            return FindCommitsWithSorting(eventSourceId.ToFilter() & commitVersion.ToFilter());
        }

        /// <inheritdoc />
        public CommittedEvents FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            return FindCommitsWithSorting(commit.ToFilter());
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

        CommittedEvents FindCommitsWithSorting(FilterDefinition<BsonDocument> filter)
        {
            //Console.WriteLine(filter.AsBson().ToJson());
            var builder = Builders<BsonDocument>.Sort;
            var sort = builder.Ascending(Constants.VERSION);
            var docs = Commits.Find(filter).Sort(sort).ToList();
            //docs.ForEach(d => Console.WriteLine(d.ToJson()));
            var commits = new List<CommittedEventStream>();
            foreach(var doc in docs)
            {
                commits.Add(doc.ToCommittedEventStream());
            } 
            return new CommittedEvents(commits);
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
        public EventSourceVersion GetVersionFor(EventSourceId eventSource)
        {
            return null;
        }

         SingleEventTypeEventStream GetEventsFromCommits(IEnumerable<CommittedEventStream> commits, ArtifactId eventType)
         {
            var events = new List<CommittedEventEnvelope>();
            foreach(var commit in commits)
            {
                events.AddRange(commit.Events.FilteredByEventType(eventType).Select(e => new CommittedEventEnvelope(commit.Sequence,e.Id,e.Metadata,e.Event)));
            }
            return new SingleEventTypeEventStream(events);
         }
    }
}