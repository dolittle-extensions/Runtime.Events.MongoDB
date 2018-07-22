using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dolittle.Runtime.Events.Store.MongoDB;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB.Specs
{
    public class a_mongo_db : IDisposable
    {
        internal MongoDbRunner _runner;
        internal string _databaseName = Guid.NewGuid().ToString();
        internal ConcurrentDictionary<Type, object> _collections = new ConcurrentDictionary<Type, object>();
        public a_mongo_db()
        {
            _runner = MongoDbRunner.Start();
            Console.WriteLine("Created mongo instance: " + _runner.GetHashCode() + " " + _databaseName);
            var client = new MongoClient(_runner.ConnectionString);
            Database = client.GetDatabase(_databaseName);
            
        }

        public IMongoDatabase Database { get; }

        internal IMongoCollection<T> GetCollection<T>()
        {
            return _collections.GetOrAdd(typeof(T), Database.GetCollection<T>(typeof(T).Name))as IMongoCollection<T>;
        }

        public static IList<T> ReadBsonFile<T>(string fileName)
        {
            string[] content = File.ReadAllLines(fileName);
            return content.Select(s => BsonSerializer.Deserialize<T>(s)).ToList();
        }
        public void Dispose()
        {
            Database.Client.DropDatabase(_databaseName);
            Console.WriteLine("Disposing of mongo instance: " + _runner.GetHashCode());
            _runner.Dispose();
        }
    }
}