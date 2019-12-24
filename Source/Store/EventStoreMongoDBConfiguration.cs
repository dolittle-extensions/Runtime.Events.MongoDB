// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Lifecycle;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Represents the configuration for the MongoDB event store.
    /// </summary>
    [SingletonPerTenant]
    public class EventStoreMongoDBConfiguration
    {
        /// <summary>
        /// Name of the Commits collection.
        /// </summary>
        public const string COMMITS = "commits";

        /// <summary>
        /// MongoDB command text for inserting a commit.
        /// </summary>
        public const string UpdateJSCommand = "function (x){ return insert_commit(x);}";

        readonly IMongoDatabase _database;
        readonly ILogger _logger;
        MongoCollectionSettings _commitSettings;

        bool _isConfigured = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStoreMongoDBConfiguration"/> class.
        /// </summary>
        /// <param name="connection">The connection for the <see cref="IMongoDatabase"/>.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance to log significant events.</param>
        public EventStoreMongoDBConfiguration(Connection connection, ILogger logger)
        {
            _database = connection.Database;
            _logger = logger;
            if (!_isConfigured)
            {
                _commitSettings = new MongoCollectionSettings { AssignIdOnInsert = false, WriteConcern = WriteConcern.Acknowledged };
                Commits = _database.GetCollection<BsonDocument>(COMMITS, _commitSettings);
                CreateIndexes();
                CreateUpdateScript();
                _isConfigured = true;
            }
        }

        /// <summary>
        /// Gets the <see cref="IMongoCollection{BsonDocument}"/> representing the Commits.
        /// </summary>
        public IMongoCollection<BsonDocument> Commits { get; }

        void CreateUpdateScript()
        {
            var sys_functions = _database.GetCollection<BsonDocument>("system.js");
            var code = CommitConstants.INSERT_COMMIT;
            try
            {
                var insert_commit_function_doc = sys_functions.Find(Builders<BsonDocument>.Filter.Eq(Constants.ID, "insert_commit")).FirstOrDefault();
                if (insert_commit_function_doc == null || insert_commit_function_doc["value"].ToString() != code)
                {
                    if (insert_commit_function_doc != null)
                    {
                        _logger.Debug($"Updating insert_commit DB: {insert_commit_function_doc["value"].ToString().Length} - {CommitConstants.INSERT_COMMIT.Length}");
                    }

                    sys_functions.UpdateOne(
                        Builders<BsonDocument>.Filter.Eq(Constants.ID, "insert_commit"),
                        Builders<BsonDocument>.Update.Set("value", new BsonJavaScript(code)),
                        new UpdateOptions { IsUpsert = true });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }

        void CreateIndexesForCommits()
        {
            var keys = Builders<BsonDocument>.IndexKeys.Descending(CommitConstants.COMMIT_ID);
            var model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument> { Unique = true });
            Commits.Indexes.CreateOne(model);

            keys = Builders<BsonDocument>.IndexKeys.Ascending(Constants.EVENTSOURCE_ID).Descending(VersionConstants.COMMIT).Ascending(Constants.EVENT_SOURCE_ARTIFACT);
            model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions<BsonDocument> { Unique = true });
            Commits.Indexes.CreateOne(model);
        }

        void CreateIndexes()
        {
            CreateIndexesForCommits();
        }
    }
}