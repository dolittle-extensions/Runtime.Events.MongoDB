using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dolittle.PropertyBags;
using Machine.Specifications;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class when_converting_property_bag_with_array_of_complex_types_to_and_from_bson
    {
        static PropertyBag original;
        static BsonDocument bson_result;
        static PropertyBag result;
        static AnEventWithArrayOfComplexTypes instance;

        Establish context = () => 
        {
            
            instance = new AnEventWithArrayOfComplexTypes(Enumerable.Range(1,10).Select(_ => new ANestedType(new AnotherNestedType(_.ToString()))));
            original = instance.ToPropertyBag();
        };
        Because of = () => 
        {
            bson_result = PropertyBagBsonSerializer.Serialize(original);
            result = PropertyBagBsonSerializer.Deserialize(bson_result);
        };

        It should_create_a_bson_document = () => bson_result.ShouldNotBeNull();
        It should_serialize_back_to_property_bag = () => result.ShouldNotBeNull();
        It should_serialize_to_the_correct_property_bag = () =>
        {
            var values = result["Values"] as IEnumerable<object>;
            values.Count().ShouldEqual(10);
            values.All(_ => _.GetType() == typeof(PropertyBag)).ShouldBeTrue();
        };
    }
}