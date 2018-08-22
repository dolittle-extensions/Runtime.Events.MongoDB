using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Runtime.Events.Store;
using System.Diagnostics.Contracts;
using Machine.Specifications;
using Dolittle.Collections;

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

        public static SingleEventTypeEventStream ShouldBeInOrder(this SingleEventTypeEventStream eventStream)
        {
            Ensure.IsNotNull(nameof(eventStream), eventStream);
            eventStream.Any().ShouldBeTrue();
            CommittedEventEnvelope prev = null;
            foreach(var evt in eventStream)
            {
                evt.CompareTo(prev).ShouldEqual(1);
                prev = evt;
            }
            return eventStream;
        }
    }
}