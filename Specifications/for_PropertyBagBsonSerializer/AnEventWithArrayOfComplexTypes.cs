using System.Collections.Generic;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class AnEventWithArrayOfComplexTypes
    {
        public IEnumerable<ANestedType> Values { get; }

        public AnEventWithArrayOfComplexTypes(IEnumerable<ANestedType> values)
        {
            Values = values;
        }
    }
}