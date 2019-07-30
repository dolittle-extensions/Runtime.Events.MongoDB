using Dolittle.Concepts;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class AnEvent : Value<AnEvent>
    {
        public AnEvent(ANestedType aNestedType)
        {
            ANestedType = aNestedType;
        }
        
        public ANestedType ANestedType { get; }
    }
}