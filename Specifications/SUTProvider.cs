using System;
using System.Linq;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Applications;
using Moq;

namespace Dolittle.Runtime.Events.Store.MongoDB.Specs
{
    public class SUTProvider : IProvideTheEventStore
    {
        private Mock<IApplicationArtifactIdentifierStringConverter> _mock_converter; 
        public SUTProvider()
        {
            _mock_converter = new Mock<IApplicationArtifactIdentifierStringConverter>();
            _mock_converter.Setup(c => c.FromString(Moq.It.IsAny<string>())).Returns((string id) => GetArtifact(id));
            _mock_converter.Setup(c => c.AsString(Moq.It.IsAny<IApplicationArtifactIdentifier>()))
                                .Returns((IApplicationArtifactIdentifier aaid) => aaid.GetHashCode().ToString());
            _mock_converter.Setup(c => c.Equals(Moq.It.IsAny<IApplicationArtifactIdentifier>()))
                                .Returns((IApplicationArtifactIdentifier aaid) => ReferenceEquals(_mock_converter.Object,aaid));
        }

        public IEventStore Build() => new test_mongodb_event_store(new a_mongo_db(),_mock_converter.Object);

        static IApplicationArtifactIdentifier GetArtifact(string id)
        {
            if(id == Dolittle.Runtime.Events.Store.Specs.given.an_event_store.event_source_artifact.GetHashCode().ToString())
                return Dolittle.Runtime.Events.Store.Specs.given.an_event_store.event_source_artifact;
            
            return Dolittle.Runtime.Events.Store.Specs.given.an_event_store.event_artifacts.Values.SingleOrDefault(v => v.GetHashCode().ToString() == id);
        }
    }
}