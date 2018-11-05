/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Collections;
using Dolittle.PropertyBags;
using Machine.Specifications;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.MongoDB.Specs.for_PropertyBagBsonSerializer
{
    public class when_converting_property_bag_with_all_supported_types_to_and_from_bson
    {
        readonly static int[] original_arrayOfInt = new int[] {1,2,3};
        readonly static Guid[] original_arrayOfGuid = new Guid[]{Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        readonly static List<int> original_listOfInt = new List<int>(){1, 2, 3};
        readonly static List<Guid> original_listOfGuid = new List<Guid>() {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        static PropertyBag original;
        static BsonDocument bson_result;

        static PropertyBag result;
        

        Establish context = () => original = new PropertyBag(
            new NullFreeDictionary<string, object> {
                {"string","a string"},
                {"int", 42},
                {"long", 42L},
                {"uint", (uint)42},
                {"ulong", (ulong)42L},
                {"int32", (Int32)42},
                {"int64", (Int64)42L},
                {"uint32", (UInt32)42},
                {"uint64", (UInt64)42L},
                {"float", 42f},
                {"double", 42d},
                {"bool", true},
                {"dateTime", DateTimeOffset.FromUnixTimeMilliseconds(1540715532995).UtcDateTime},
                {"dateTimeOffset", DateTimeOffset.FromUnixTimeMilliseconds(1540715541241) },
                {"guid", Guid.NewGuid()},
                {"arrayOfInt", original_arrayOfInt},
                {"arrayOfGuid", original_arrayOfGuid},
                {"listOfInt", original_listOfInt},
                {"listOfGuid", original_listOfGuid}
            }
        );

        Because of = () => 
        {
            bson_result = PropertyBagBsonSerializer.Serialize(original);
            result = PropertyBagBsonSerializer.Deserialize(bson_result);
        };

        It should_create_a_bson_document = () => bson_result.ShouldNotBeNull();
        It should_serialize_back_to_property_bag = () => result.ShouldNotBeNull();

        It should_serialize_to_the_correct_property_bag = () => 
        {
            result["string"].ShouldEqual(original["string"]);
            result["int"].ShouldEqual(original["int"]);
            result["long"].ShouldEqual(original["long"]);
            uint.Parse(result["uint"].ToString()).ShouldEqual(original["uint"]);
            ulong.Parse(result["ulong"].ToString()).ShouldEqual(original["ulong"]);
            Int32.Parse(result["int32"].ToString()).ShouldEqual(original["int32"]);
            Int64.Parse(result["int64"].ToString()).ShouldEqual(original["int64"]);
            UInt32.Parse(result["uint32"].ToString()).ShouldEqual(original["uint32"]);
            UInt64.Parse(result["uint64"].ToString()).ShouldEqual(original["uint64"]);
            float.Parse(result["float"].ToString()).ShouldEqual(original["float"]);
            double.Parse(result["double"].ToString()).ShouldEqual(original["double"]);
            result["bool"].ShouldEqual(original["bool"]);
            result["dateTime"].ShouldEqual(original["dateTime"]);
            new DateTimeOffset(long.Parse(result["dateTimeOffset"].ToString()), new TimeSpan(0,0,0)).ShouldEqual(original["dateTimeOffset"]);
            result["guid"].ShouldEqual(original["guid"]);

            var result_arrayOfInt = CreateArrayOf<int>(result["arrayOfInt"], (object obj) => int.Parse(obj.ToString())).ToArray();
            result_arrayOfInt.ShouldContainOnly(original_arrayOfInt);
            var result_arrayOfGuid = CreateArrayOf<Guid>(result["arrayOfGuid"], (object obj) => Guid.Parse(obj.ToString())).ToArray();
            result_arrayOfGuid.ShouldContainOnly(original_arrayOfGuid);

            var result_listOfInt = CreateArrayOf<int>(result["listOfInt"], (object obj) => int.Parse(obj.ToString())).ToList();
            result_listOfInt.ShouldContainOnly(original_listOfInt);
            var result_listOfGuid = CreateArrayOf<Guid>(result["listOfGuid"], (object obj) => Guid.Parse(obj.ToString())).ToList();
            result_listOfGuid.ShouldContainOnly(original_listOfGuid);
        };

        static IEnumerable<TResult> CreateArrayOf<TResult>(object arrayObject, Func<object, TResult> converterFunc) => 
            ((System.Collections.IEnumerable) arrayObject)
                .Cast<object>()
                .Select(converterFunc);
    }
}