/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Collections;
using Dolittle.PropertyBags;
using Dolittle.Reflection;
using Dolittle.Time;
using MongoDB.Bson;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Defines a set of functions for serializing <see cref="PropertyBag"/> to and from <see cref="BsonDocument"/>
    /// </summary>
    public static class PropertyBagBsonSerializer
    {
        /// <summary>
        /// Serialize a <see cref="BsonDocument"/> to a <see cref="PropertyBag"/>
        /// </summary>
        /// <param name="doc"></param>
        public static PropertyBag Deserialize(BsonDocument doc)
        {
            var bsonAsDictionary = doc.ToDictionary();
            return ToPropertyBag(bsonAsDictionary);
        }

        private static PropertyBag ToPropertyBag(Dictionary<string, object> target)
        {
            var nonNullDictionary = new NullFreeDictionary<string,object>();
            target.ForEach(kvp =>
            {
                if (kvp.Value == null) return;
                
                var valueType = kvp.Value.GetType();
                if (valueType == typeof(object[]))
                {
                    var instances = (from object obj in kvp.Value as IEnumerable select IsComplexType(obj) ? ToPropertyBag(obj as Dictionary<string, object>) : obj).ToList();
                    nonNullDictionary.Add(new KeyValuePair<string, object>(kvp.Key, instances));
                }
                else
                {
                    nonNullDictionary.Add(IsComplexType(kvp.Value)
                        ? new KeyValuePair<string, object>(kvp.Key, ToPropertyBag(kvp.Value as Dictionary<string, object>))
                        : kvp);
                }
            });
            return new PropertyBag(nonNullDictionary);
        }
        
        
        /// <summary>
        /// Serialize a <see cref="PropertyBag"/> to a <see cref="BsonDocument"/>
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <returns></returns>
        public static BsonDocument Serialize(PropertyBag propertyBag)
        {
            var doc = new BsonDocument();
            propertyBag.ForEach(kvp => {
                doc.Add(new BsonElement(kvp.Key, ValueAsBsonValue(kvp.Value)));
            });
            return doc;
        }

        static BsonValue ValueAsBsonValue(object value)
        {
            var type = value.GetType();
            if (type.Equals(typeof(PropertyBag))) return Serialize(value as PropertyBag);
            if (type.IsEnumerable()) return EnumerableAsBsonArray(value as IEnumerable);
            if (type.Equals(typeof(Guid))) return new BsonBinaryData((Guid)value);
            if (type.Equals(typeof(DateTime))) return new BsonInt64(((DateTime)value).ToUnixTimeMilliseconds());
            if (type.Equals(typeof(DateTimeOffset))) return new BsonInt64(((DateTimeOffset)value).ToUnixTimeMilliseconds());
            return BsonValue.Create(value);            
        }

        static BsonArray EnumerableAsBsonArray(IEnumerable enumerable)
        {
            var array = new BsonArray();
            if (enumerable != null)
            {
                foreach (var element in enumerable)
                {
                    array.Add(ValueAsBsonValue(element));
                }
            }
            return array;
        }
        
        static bool IsComplexType(object obj)
        {
            return obj.GetType() == typeof(Dictionary<string, object>);
        }
    }
}
