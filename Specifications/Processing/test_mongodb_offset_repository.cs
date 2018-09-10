using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dolittle.Runtime.Events.Store.MongoDB;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Dolittle.Applications;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB.Specs;

namespace Dolittle.Runtime.Events.Processing.MongoDB.Specs
{

    public class test_mongodb_offset_repository : EventProcessorOffsetRepository
    {
        private readonly a_mongo_db _database_runner;
        public test_mongodb_offset_repository(a_mongo_db database_runner, ILogger logger): base(database_runner.Database, logger)
        {
            _database_runner = database_runner;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _database_runner.Dispose();
                disposedValue = true;
            }
        }
    }
}