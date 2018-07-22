using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Runtime.Events.Store;
using System.Diagnostics.Contracts;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs
{

    public static class ShouldExtensions 
    {
        public static CommittedEventStream ShouldCorrespondTo(this CommittedEventStream result, UncommittedEventStream uncommittedEventStream)
        {
            Ensure.IsNotNull(nameof(uncommittedEventStream), uncommittedEventStream);
            Ensure.IsNotNull(nameof(result),result);
            Ensure.ArgumentPropertyIsNotNull(nameof(uncommittedEventStream), "Events", uncommittedEventStream.Events);
            Ensure.ArgumentPropertyIsNotNull(nameof(result), "Events", result.Events);

            result.Events.ShouldContainOnly(uncommittedEventStream.Events);
            result.Source.ShouldEqual(uncommittedEventStream.Source);
            return result;
        }
    }
}