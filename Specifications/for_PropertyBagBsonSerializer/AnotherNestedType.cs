using Dolittle.Concepts;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class AnotherNestedType : Value<AnotherNestedType>
    {
        public AnotherNestedType(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}