// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Processing.InMemory.Specs;
using Dolittle.Runtime.Events.Specs.MongoDB;

namespace Dolittle.Runtime.Events.Processing.MongoDB.Specs
{
    public class SUTProvider : IProvideTheOffsetRepository
    {
        public IEventProcessorOffsetRepository Build() => new test_mongodb_offset_repository(new a_mongo_db_connection(), given.a_logger());
    }
}