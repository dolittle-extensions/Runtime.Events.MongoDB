// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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