// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Specs.MongoDB;

namespace Dolittle.Runtime.Events.Relativity.Specs.MongoDB
{
    public class SUTProvider : IProvideGeodesics
    {
        public IGeodesics Build() => new test_mongodb_geodesics(new a_mongo_db_connection(), Dolittle.Runtime.Events.Specs.MongoDB.given.a_logger());
    }
}