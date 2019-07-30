/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using Dolittle.Collections;
using Dolittle.Time;
using Dolittle.PropertyBags;
using Machine.Specifications;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class when_converting_property_bag_with_nested_complex_type_to_and_from_bson
    {
        static PropertyBag original;
        static BsonDocument bson_result;
        static PropertyBag result;
        static AnEvent instance;

        Establish context = () => 
        {
            instance = new AnEvent(new ANestedType(new AnotherNestedType("myNestedType")));
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
            var nested = result["ANestedType"] as PropertyBag;
            var anotherNested = nested["AnotherNestedType"] as PropertyBag;
            anotherNested["Key"].ShouldEqual(instance.ANestedType.AnotherNestedType.Key);
        };
    }
}