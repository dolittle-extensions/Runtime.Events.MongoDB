/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using Dolittle.Applications;
using Dolittle.Collections;
using Dolittle.PropertyBags;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events;
using MongoDB.Bson;
using Dolittle.Security;

namespace Dolittle.Runtime.Events.MongoDB
{

    /// <summary>
    /// Extensions to convert <see cref="BsonValue">Bson Values</see> into domain specific types and dotnet types
    /// </summary>
    public static class BsonValueExtensions 
    {
        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="DateTimeOffset" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="DateTimeOffset" /></returns>
        public static DateTimeOffset AsDateTimeOffset(this BsonValue value)
        {
            return new DateTimeOffset(value.AsInt64, new TimeSpan(0,0,0));
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="CommitId" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="CommitId" /></returns>
        public static CommitId ToCommitId(this BsonValue value)
        {
            return value.AsGuid;
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="UInt64" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="UInt64" /></returns>
        public static ulong ToUlong(this BsonValue value)
        {
            return Convert.ToUInt64(value.ToDouble());
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="UInt32" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="UInt32" /></returns>
        public static uint ToUint(this BsonValue value)
        {
            return Convert.ToUInt32(value.ToInt64());
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into an <see cref="EventStream" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="EventStream" /></returns>
        public static EventStream ToEventStream(this BsonValue value)
        {
            var list = new List<EventEnvelope>();
            foreach(var val in value.AsBsonArray)
            {
                list.Add(val.AsBsonDocument.ToEventEnvelope());
            }
            return new EventStream(list);
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="PropertyBag" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /></param>
        /// <returns>The corresponding <see cref="PropertyBag" /></returns>        
        public static PropertyBag ToPropertyBag(this BsonValue value)
        {
            return PropertyBagBsonSerializer.Deserialize(value.AsBsonDocument);
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into an <see cref="OriginalContext" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /> that is the source</param>
        /// <returns>An <see cref="OriginalContext" /> with the values from the <see cref="BsonValue" /></returns>
        public static OriginalContext ToOriginalContext(this BsonValue value)
        {
            try
            {
                var doc = value.AsBsonDocument;
                var application = doc[EventConstants.APPLICATION].AsGuid;
                var boundedContext = doc[EventConstants.BOUNDED_CONTEXT].AsGuid;
                var tenant = doc[EventConstants.TENANT].AsGuid;
                var environment = doc[EventConstants.ENVIRONMENT].AsString;
                var claims = doc[EventConstants.CLAIMS].AsBsonArray.ToClaims();
                return new OriginalContext(application,boundedContext,tenant,environment,claims);
            }
            catch(Exception)
            {
                Console.WriteLine(value.ToJson());
                throw;
            }
        }

        /// <summary>
        /// Converts a <see cref="BsonArray" /> into <see cref="Claims" />
        /// </summary>
        /// <param name="array">The <see cref="BsonArray" /> that is the source</param>
        /// <returns><see cref="Claims" /> with the values from the <see cref="BsonArray" /></returns>
        public static Claims ToClaims(this BsonArray array)
        {
            return new Claims(array.Select(_ => _.ToClaim()).ToList());
        }

        /// <summary>
        /// Converts a <see cref="BsonValue" /> into a <see cref="Claim" />
        /// </summary>
        /// <param name="value">The <see cref="BsonValue" /> that is the source</param>
        /// <returns><see cref="Claim" /> with the values from the <see cref="BsonValue" /></returns>
        public static Claim ToClaim(this BsonValue value)
        {
            var doc = value.AsBsonDocument;
            var name = doc[EventConstants.CLAIM_NAME].AsString;
            var val = doc[EventConstants.CLAIM_VALUE].AsString;
            var valueType = doc[EventConstants.CLAIM_VALUE_TYPE].AsString;
            return new Claim(name, val, valueType);
        }
    }
}