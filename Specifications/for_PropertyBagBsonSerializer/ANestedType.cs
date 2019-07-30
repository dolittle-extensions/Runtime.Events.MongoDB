using Dolittle.Concepts;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class ANestedType : Value<ANestedType>
    {
        public ANestedType(AnotherNestedType anotherNestedType)
        {
            AnotherNestedType = anotherNestedType;
        }
        public AnotherNestedType AnotherNestedType { get; }
    }
}