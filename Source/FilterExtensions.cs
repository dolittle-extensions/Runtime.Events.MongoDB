using MongoDB.Driver;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Extensions for converting to a <see cref="FilterDefinition{BsonDocument}" />
    /// </summary>
    public static class FilterExtensions
    {
        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventSourceId" /> supplied
        /// </summary>
        /// <param name="id">An <see cref="EventSourceId" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="EventSourceId" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this EventSourceId id)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.EVENTSOURCE_ID, id.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitId" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitId" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitId" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitId commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(CommitConstants.COMMIT_ID, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitVersion" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitVersion" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitVersion" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitVersion commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Gte(VersionConstants.COMMIT, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitSequenceNumber" /> supplied
        /// </summary>
        /// <param name="commit">A <see cref="CommitSequenceNumber" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="CommitSequenceNumber" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this CommitSequenceNumber commit)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Gte(Constants.ID, commit.Value);
            return filter;
        }

        /// <summary>
        /// Builds a <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="VersionedEventSource" /> supplied
        /// </summary>
        /// <param name="version">A <see cref="VersionedEventSource" /></param>
        /// <returns>A <see cref="FilterDefinition{BsonDocument}" /> corresponding to the <see cref="VersionedEventSource" /></returns>
        public static FilterDefinition<BsonDocument> ToFilter(this VersionedEventSource version)
        {
            var builder = Builders<BsonDocument>.Filter;                   
            var filter = builder.Eq(Constants.EVENTSOURCE_ID, version.EventSource.Value) & 
                            builder.Eq(VersionConstants.COMMIT, version.Version.Commit);
            return filter;
        }
    }
}