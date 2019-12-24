// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Specs.MongoDB;
using Dolittle.Runtime.Events.Store.Specs;

namespace Dolittle.Runtime.Events.Store.MongoDB.Specs
{
    public class SUTProvider : IProvideTheEventStore
    {
        public IEventStore Build() => new test_mongodb_event_store(new a_mongo_db_connection(), given.a_logger());
    }
}