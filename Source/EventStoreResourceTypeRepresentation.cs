// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Dolittle.ResourceTypes;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <inheritdoc/>
    public class EventStoreResourceTypeRepresentation : IRepresentAResourceType
    {
        static readonly IDictionary<Type, Type> _bindings = new Dictionary<Type, Type>
        {
            { typeof(Dolittle.Runtime.Events.Store.IEventStore), typeof(Store.MongoDB.EventStore) },
            { typeof(Dolittle.Runtime.Events.Relativity.IGeodesics), typeof(Relativity.MongoDB.Geodesics) },
            { typeof(Dolittle.Runtime.Events.Processing.IEventProcessorOffsetRepository), typeof(Processing.MongoDB.EventProcessorOffsetRepository) }
        };

        /// <inheritdoc/>
        public ResourceType Type => "eventStore";

        /// <inheritdoc/>
        public ResourceTypeImplementation ImplementationName => "MongoDB";

        /// <inheritdoc/>
        public Type ConfigurationObjectType => typeof(EventStoreConfiguration);

        /// <inheritdoc/>
        public IDictionary<Type, Type> Bindings => _bindings;
    }
}