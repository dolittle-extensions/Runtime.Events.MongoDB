using System;
using System.Linq;

namespace EventStore.MongoDB
{

    public static class DateTimeExtensions 
    {
        public static DateTimeOffset ToDateTimeOffset(this DateTime? date)
        {
            return date.HasValue ? date.Value : DateTimeOffset.MinValue;
        }
    }
}