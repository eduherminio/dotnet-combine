using System;
using System.ComponentModel;

namespace FileParser
{
    internal static class StringConverter
    {
        /// <summary>
        /// Converts strings to basic, nullable types
        /// Optional parameter: an already known typeConverter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="typeConverter"></param>
        /// <returns></returns>
        public static T Convert<T>(string str, TypeConverter typeConverter = null)
        {
            if (typeof(T).IsPrimitive)
            {
                return typeConverter == null
                    ? TConverter.ChangeType<T>(str)
                    : TConverter.ChangeType<T>(str, typeConverter);
            }
            else // Avoids exception if T is an object
            {
                object o = str;
                return (T)o;
            }
        }

        public static TypeConverter GetConverter<T>()
        {
            if (typeof(T).IsPrimitive)
            {
                return TConverter.GetTypeConverter(typeof(T));
            }
            else
            {
                throw new NotSupportedException($"Converter for {typeof(T).Name} yet to be implemented");
            }
        }
    }
}
