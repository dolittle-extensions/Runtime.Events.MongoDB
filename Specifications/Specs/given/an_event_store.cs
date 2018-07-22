using System;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Moq;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Applications;

namespace Dolittle.Runtime.Events.Store.Specs.given
{
    public class an_event_store
    {
        protected static IApplicationArtifactIdentifier event_source_artifact => new Mock<IApplicationArtifactIdentifier>().Object;      

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