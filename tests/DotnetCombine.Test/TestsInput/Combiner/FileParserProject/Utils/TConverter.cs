using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace FileParser
{
    /// <summary>
    /// Provides generic type conversions for basic types, including nullable ones
    /// Original method by Tuna Toksoz
    /// Sources:
    /// https://stackoverflow.com/questions/8625/generic-type-conversion-from-string
    /// http://web.archive.org/web/20101214042641/http://dogaoztuzun.com/post/C-Generic-Type-Conversion.aspx
    /// </summary>
    internal static class TConverter
    {
        internal static T ChangeType<T>(object value, TypeConverter typeConverter = null)
        {
            return typeConverter == null
                    ? (T)ChangeType(typeof(T), value)
                    : (T)typeConverter.ConvertFrom(value);
        }

        private static object ChangeType(Type t, object value)
        {
            if (t == typeof(double))
            {
                return ParseDouble(value);
            }
            else
            {
                TypeConverter tc = TypeDescriptor.GetConverter(t);

                return tc.ConvertFrom(value);
            }
        }

        private static object ParseDouble(object value)
        {
            double result;

            string doubleAsString = value.ToString();

            if (doubleAsString == null)
            {
                throw new ParsingException($"Error parsing value as double");
            }

            IEnumerable<char> doubleAsCharList = doubleAsString.ToList();

            if (doubleAsCharList.Count(ch => ch == '.' || ch == ',') <= 1)
            {
                double.TryParse(doubleAsString.Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out result);
            }
            else
            {
                if (doubleAsCharList.Count(ch => ch == '.') <= 1
                    && doubleAsCharList.Count(ch => ch == ',') > 1)
                {
                    double.TryParse(doubleAsString.Replace(",", string.Empty),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out result);
                }
                else if (doubleAsCharList.Count(ch => ch == ',') <= 1
                    && doubleAsCharList.Count(ch => ch == '.') > 1)
                {
                    double.TryParse(doubleAsString.Replace(".", string.Empty).Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out result);
                }
                else
                {
                    throw new ParsingException($"Error parsing {doubleAsString} as double, try removing thousand separators (if any)");
                }
            }

            return result as object;
        }

        internal static TypeConverter GetTypeConverter(Type t)
        {
            return TypeDescriptor.GetConverter(t);
        }

        /// <summary>
        /// Currently unused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TC"></typeparam>
        internal static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
        }
    }
}
