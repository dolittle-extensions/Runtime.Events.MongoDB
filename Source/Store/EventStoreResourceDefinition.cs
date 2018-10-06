using System;
using Dolittle.Resources;

namespace Dolittle.Runtime.Events.Store
{
    /// <inheritdoc/>
    public class EventStoreResourceDefinition : ICanDefineAResource
    {
        /// <inheritdoc/>
        public ResourceType ResourceType => new ResourceType{Value = "eventStore"};
        /// <inheritdoc/>
        public ResourceTypeName ResourceTypeName => new ResourceTypeName{Value = "MongoDB"};
        /// <inheritdoc/>
        public Type ConfigurationObjectType => typeof(EventStoreConfiguration);
    }
}