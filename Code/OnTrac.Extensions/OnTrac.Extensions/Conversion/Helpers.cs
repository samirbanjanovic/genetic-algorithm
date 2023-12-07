using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OnTrac.Extensions.Conversion
{
    public static class Helpers
    {
        /// <summary>
        /// Attempts to cast the input database type to desired .NET type.
        /// If the value is NULL the method retunrs the default value for TDesired.
        /// If a cast cannot be performed the default value is returned as well
        /// </summary>
        /// <typeparam name="TDesired">Desired type to cast to</typeparam>
        /// <param name="value">item to cast</param>
        /// <returns>Cast </returns>
        public static TDesired TryCastOrDefault<TDesired>(this object value)
        {
            Type sourcetype = value.GetType();
            TypeConverter converter;
            bool inputIsString = sourcetype == typeof(string);

            if (value == null || Convert.IsDBNull(value))
            {
                return default(TDesired);
            }
            else if (typeof(TDesired) == sourcetype)
            {
                return (TDesired)value;
            }
           
            if (typeof(TDesired) == typeof(string))
            {//convert T ----> string
                return (TDesired)Convert.ChangeType(value, typeof(string));
            }

            if (inputIsString)
            {//convert string ----> T
                converter = TypeDescriptor.GetConverter(typeof(TDesired));
                if (converter.CanConvertFrom(sourcetype))
                {
                    if (converter.IsValid(value))
                        return (TDesired)converter.ConvertTo(value, typeof(TDesired));
                }
            }

            return default(TDesired);
        }
    }
}
