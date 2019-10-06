/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using System;
using Dolittle.Lifecycle;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// 
    /// </summary>
    [SingletonPerTenant]
    public class EventStoreMongoDBConfiguration 
    {
        /// <summary>
        /// Name of the Commits collection
        /// </summary>
        public const string COMMITS = "commits"; 

        /// <summary>
        /// MongoDB command text for inserting a commit
        /// </summary>
        public const string UpdateJSCommand = "function (x){ return insert_commit(x);}";

        readonly IMongoDatabase _database;
        readonly ILogger _logger;
        MongoCollectionSettings _commitSettings;

        bool _isConfigured = false;

        /// <summary>
        /// Instantiates an instance of the EventStore
        /// </summary>
        /// <param name="connection">The connection for the <see cref="IMongoDatabase"/></param>
        /// <param name="logger">An <see cref="ILogger"/> instance to log significant events</param>
        public EventStoreMongoDBConfiguration(Connection connection, ILogger logger)
        {
            _database = connection.Database;
            _logger = logger;
            Bootstrap();
        }
        /// <summary>
        /// Access to the IMongoCollection{BsonDocument} representing the Commits
        /// </summary>
        /// <returns><see cref="IMongoCollection{BsonDocument}"/> representing the Commits"</returns>
        public IMongoCollection<BsonDocument> Commits => _database.GetCollection<BsonDocument>(COMMITS, _commitSettings);

        void Bootstrap()
        {
            if(!_isConfigured)
            {
                _commitSettings = new MongoCollectionSettings{ AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
                CreateIndexes();
                CreateUpdateScript();
                _isConfigured = true;
            }

        }

        void CreateUpdateScript()
        {
            var sys_functions = _database.GetCollection<BsonDocument>("system.js");
            var code = CommitConstants.INSERT_COMMIT;
            var commit_function_doc = new BsonDocument("value", new BsonJavaScript(code));
            try
            {
                
                var insert_commit_function_doc = sys_functions.Find(Builders<BsonDocument>.Filter.Eq(Constants.ID,"insert_commit")).FirstOrDefault();
                if(insert_commit_function_doc == null || insert_commit_function_doc["value"].ToString() != code)
                {
                    if(insert_commit_function_doc != null)
                    {
                        _logger.Debug($"Updating insert_commit DB: {insert_commit_function_doc["value"].ToString().Length} - {CommitConstants.INSERT_COMMIT.Length}");

                    } 
                    sys_functions.UpdateOne(Builders<BsonDocument>.Filter.Eq(Constants.ID,"insert_commit"),  
                                                        Builders<BsonDocument>.Update.Set("value", new BsonJavaScript(code)), 
                                                        new UpdateOptions{ IsUpsert = true });
                }
            } 
            catch(Exception ex) 
            {
                _logger.Error(ex.ToString());
            }
            
        }

        void CreateIndexesForCommits()
        {
            var keys = Builders<BsonDocument>.IndexKeys.Descending(CommitConstants.COMMIT_ID);
            var model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument>{ Unique = true });
            Commits.Indexes.CreateOne(model);

            keys = Builders<BsonDocument>.IndexKeys.Ascending(Constants.EVENTSOURCE_ID).Descending(VersionConstants.COMMIT).Ascending(Constants.EVENT_SOURCE_ARTIFACT);
            model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument>{ Unique = true });
            Commits.Indexes.CreateOne(model);
        }

        void CreateIndexes()
        {
            CreateIndexesForCommits();
        }
    }
}