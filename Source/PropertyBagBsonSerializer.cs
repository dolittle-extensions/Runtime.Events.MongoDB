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
using MongoDB.Bson.Serialization;

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
            var nonNullDictionary = new NullFreeDictionary<string,object>();
            doc.ForEach(kvp => {
                var value = BsonValueAsValue(kvp.Value);
                if (value != null) nonNullDictionary.Add(kvp.Name, value);
            });
            return new PropertyBag(nonNullDictionary);
        }

        static object BsonValueAsValue(BsonValue value)
        {
            if (value.IsBsonArray) return value.AsBsonArray.Select(BsonValueAsValue);
            if (value.IsBsonDocument) return Deserialize(value.AsBsonDocument);
            return BsonTypeMapper.MapToDotNetValue(value);
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
            if (type.IsEnumerable()) return new BsonArray((value as IEnumerable<object>).Select(ValueAsBsonValue));
            if (type.Equals(typeof(PropertyBag))) return Serialize(value as PropertyBag);
            if (type.Equals(typeof(Guid))) return new BsonBinaryData((Guid)value);
            if (type.Equals(typeof(DateTime))) return new BsonInt64(((DateTime)value).ToUnixTimeMilliseconds());
            if (type.Equals(typeof(DateTimeOffset))) return new BsonInt64(((DateTimeOffset)value).ToUnixTimeMilliseconds());
            return BsonValue.Create(value);            
        }
    }
}