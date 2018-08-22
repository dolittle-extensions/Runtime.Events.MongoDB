using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Moq;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Artifacts;

namespace Dolittle.Runtime.Events.Store.Specs.given
{
    public class an_event_store
    {
        public static readonly ArtifactId event_source_artifact = Guid.NewGuid();  
        public static ConcurrentDictionary<Type,ArtifactId> event_artifacts = new ConcurrentDictionary<Type, ArtifactId>();    

        static Type _sut_provider_type;
        protected static Func<IEventStore> get_event_store = () => {
            if(_sut_provider_type == null){
                var asm = Assembly.GetExecutingAssembly();      
                _sut_provider_type = asm.GetExportedTypes().Where(t => t.IsClass && typeof(IProvideTheEventStore).IsAssignableFrom(t)).Single();
            }
            var factory = Activator.CreateInstance(_sut_provider_type) as IProvideTheEventStore;
            return factory.Build();
        };
    }
}