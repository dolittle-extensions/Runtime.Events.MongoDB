using System;
using System.Collections;
using Dolittle.Collections;
using Dolittle.PropertyBags;
using Dolittle.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Dolittle.Runtime.Events.MongoDB
{
    internal static class PropertyBagBsonSerializer
    {

        internal static PropertyBag Deserialize(BsonDocument doc)
        {
            var bsonAsDictionary = doc.ToDictionary();
            var nonNullDictionary = new NullFreeDictionary<string,object>();
            bsonAsDictionary.ForEach(kvp =>
            {
                if(kvp.Value != null)
                    nonNullDictionary.Add(kvp);
            });
            var propertyBag = new PropertyBag(nonNullDictionary);
            return propertyBag;
        }

        internal static BsonDocument Serialize(PropertyBag propertyBag)
        {
            var doc = new BsonDocument();
            propertyBag.ForEach(kvp => {
                doc.Add(new BsonElement(kvp.Key, ValueAsBsonValue(kvp.Value)));
            });
            return doc;
        }

        static BsonValue ValueAsBsonValue(object value)
        {
            var valueType = value.GetType();
            if (valueType.IsEnumerable())
            {
                var bsonValue = new BsonArray();
                var enumerableValue = value as IEnumerable;
                foreach (var obj in enumerableValue)
                {
                    bsonValue.Add(ValueAsBsonValue(obj));
                }
                return bsonValue;
            }
            else if (valueType.Equals(typeof(Guid))) return new BsonBinaryData((Guid)value);
            else return BsonValue.Create(value);
            
        }
    }
}