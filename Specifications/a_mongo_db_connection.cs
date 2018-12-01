using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dolittle.Resources.Configuration;
using Dolittle.Runtime.Events.MongoDB;
using Dolittle.Runtime.Events.Store.MongoDB;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Moq;
namespace Dolittle.Runtime.Events.Specs.MongoDB
{
    public class a_mongo_db_connection : IDisposable
    {

        internal MongoDbRunner _runner;
        
        internal string _databaseName = Guid.NewGuid().ToString();
        internal ConcurrentDictionary<Type, object> _collections = new ConcurrentDictionary<Type, object>();

        public a_mongo_db_connection()
        {
            _runner = MongoDbRunner.Start();
            var configurationForMock = new Mock<IConfigurationFor<EventStoreConfiguration>>();
            configurationForMock.Setup(_ => _.Instance).Returns(new EventStoreConfiguration
            {
                ConnectionString = _runner.ConnectionString,
                Database = _databaseName
            });
            
            Connection = new Connection(configurationForMock.Object);
        }

        public Connection Connection {get; }
        internal IMongoCollection<T> GetCollection<T>()
        {
            return _collections.GetOrAdd(typeof(T), Connection.Database.GetCollection<T>(typeof(T).Name))as IMongoCollection<T>;
        }

        public static IList<T> ReadBsonFile<T>(string fileName)
        {
            string[] content = File.ReadAllLines(fileName);
            return content.Select(s => BsonSerializer.Deserialize<T>(s)).ToList();
        }
        public void Dispose()
        {
            Connection.Database.Client.DropDatabase(_databaseName);
            _runner.Dispose();
        }
    }
}