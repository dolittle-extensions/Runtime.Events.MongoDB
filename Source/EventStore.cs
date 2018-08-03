using Dolittle.Runtime.Events.Store;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Dolittle.Applications;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    public class EventStore : IEventStore
    {
        public object lock_object = new object();
        public const string COMMITS = "commits"; 
        public const string VERSIONS = "versions";
        public const string SNAPSHOTS = "shapshots";

        private IMongoDatabase _database;
        private MongoCollectionSettings _commitSettings;
        private MongoCollectionSettings _versionSettings;
        private MongoCollectionSettings _snapshotSettings;

        private string _updateJSCommand ="function (x){ return insert_commit(x);}";

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

        public CommittedEvents Fetch(EventSourceId eventSourceId)
        {
           return FindCommitsWithSorting(eventSourceId.ToFilter()); 
        }

        public CommittedEvents FetchFrom(EventSourceId eventSourceId, CommitVersion commitVersion)
        {
            return FindCommitsWithSorting(eventSourceId.ToFilter() & commitVersion.ToFilter());
        }

        public CommittedEvents FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            return FindCommitsWithSorting(commit.ToFilter());
        }

        #region IDisposable Support
        protected bool disposedValue = false; // To detect redundant calls

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
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


        protected virtual T Do<T>(Func<T> callback)
        {
            T results = default(T);
            Do(() => { results = callback(); });
            return results;
        }

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
            var builder = Builders<BsonDocument>.Sort;
            var sort = builder.Ascending(Constants.VERSION);
            var docs = Commits.Find(filter).Sort(sort).ToList();
            var commits = new List<CommittedEventStream>();
            foreach(var doc in docs)
            {
                commits.Add(doc.ToCommittedEventStream());
            } 
            return new CommittedEvents(commits);
        } 
    }
}