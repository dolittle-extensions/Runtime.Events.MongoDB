/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
 
using System;
using System.Collections.Generic;
using Dolittle.Resources;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <inheritdoc/>
    public class EventStoreResourceTypeRepresentation : IRepresentAResourceType
    {
        IDictionary<Type, Type> _bindings;
        
        /// <inheritdoc/>
        public ResourceType Type => "eventStore";
        /// <inheritdoc/>
        public ResourceTypeImplementation ImplementationName => "MongoDB";
        /// <inheritdoc/>
        public Type ConfigurationObjectType => typeof(EventStoreConfiguration);
        /// <inheritdoc/>
        public IDictionary<Type, Type> Bindings {
            get 
            {
                if (_bindings == null) 
                    InitializeBindings();
                
                return _bindings;
           }
        }

        void InitializeBindings()
        {
            _bindings = new Dictionary<Type, Type>();
            _bindings.Add(typeof(Dolittle.Runtime.Events.Store.IEventStore), typeof(Dolittle.Runtime.Events.Store.MongoDB.EventStore));
            _bindings.Add(typeof(Dolittle.Runtime.Events.Relativity.IGeodesics), typeof(Dolittle.Runtime.Events.Relativity.MongoDB.Geodesics));
            _bindings.Add(typeof(Dolittle.Runtime.Events.Processing.IEventProcessorOffsetRepository), typeof(Dolittle.Runtime.Events.Processing.MongoDB.EventProcessorOffsetRepository));
        }
    }
}