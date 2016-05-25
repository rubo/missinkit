// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using Foundation;

namespace MissinKit.Utilities
{
    public static class NSDateUtility
    {
        private static readonly DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this NSDate date)
        {
            return ReferenceDate.AddSeconds(date.SecondsSinceReferenceDate).ToLocalTime();
        }

        public static NSDate ToNSDate(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

            return (NSDate) dateTime;
        }
    }
}
