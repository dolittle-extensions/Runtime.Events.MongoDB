// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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