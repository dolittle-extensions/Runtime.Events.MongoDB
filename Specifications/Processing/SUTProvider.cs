using System;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Specs.MongoDB;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Processing.InMemory.Specs;
using Moq;

namespace Dolittle.Runtime.Events.Processing.MongoDB.Specs
{
    public class SUTProvider : IProvideTheOffsetRepository
    {
        public IEventProcessorOffsetRepository Build() => new test_mongodb_offset_repository(new a_mongo_db_connection(),given.a_logger());
    }
}