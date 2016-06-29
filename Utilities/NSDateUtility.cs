// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using Foundation;

namespace MissinKit.Utilities
{
    public static class NSDateUtility
    {
        private static readonly DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts the current <see cref="NSDate"/> object to its equivalent <see cref="DateTime"/> representation.
        /// </summary>
        /// <param name="date">An object that represents the value to convert.</param>
        /// <returns>
        /// An object that represents the value of the current <see cref="NSDate"/> object converted to <see cref="DateTime"/>.
        /// </returns>
        public static DateTime ToDateTime(this NSDate date)
        {
            return ReferenceDate.AddSeconds(date.SecondsSinceReferenceDate).ToLocalTime();
        }

        /// <summary>
        /// Converts the current <see cref="DateTime"/> object to its equivalent <see cref="NSDate"/> representation.
        /// </summary>
        /// <param name="dateTime">An object that represents the value to convert.</param>
        /// <returns>
        /// An object that represents the date and time of the current <see cref="DateTime"/> object converted to <see cref="NSDate"/>.
        /// </returns>
        public static NSDate ToNSDate(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

            return (NSDate) dateTime;
        }
    }
}
