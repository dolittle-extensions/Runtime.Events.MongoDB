using Dolittle.Runtime.Events.Store.Specs;

namespace Dolittle.Runtime.Events.Store.MongoDB.Specs
{
    public class SUTProvider : IProvideTheEventStore
    {
        public IEventStore Build() => new test_mongodb_event_store(new a_mongo_db());
    }
}