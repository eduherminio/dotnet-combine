using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileParser
{
    /// <summary>
    /// Implementation based on Queue&lt;string&gt;
    /// </summary>
    public class ParsedLine : Queue<string>, IParsedLine
    {
        public bool Empty { get { return Count == 0; } }

        /// <summary>
        /// Parses a line
        /// </summary>
        /// <param name="parsedLine"></param>
        public ParsedLine(IEnumerable<string> parsedLine)
            : base(new Queue<string>(parsedLine))
        {
        }

        /// <summary>
        /// Parses a line
        /// </summary>
        /// <param name="parsedLine"></param>
        public ParsedLine(Queue<string> parsedLine)
            : base(parsedLine)
        {
        }

        public T NextElement<T>()
        {
            if (!Empty)
            {
                return Extract<T>();
            }

            throw new ParsingException("End of ParsedLine reached");
        }

        public T PeekNextElement<T>()
        {
            if (!Empty)
            {
                return Peek<T>();
            }

            throw new ParsingException("End of ParsedLine reached");
        }

        public T ElementAt<T>(int index)
        {
            ValidateSupportedType<T>();

            string element = this.ElementAt(index);

            return StringConverter.Convert<T>(element);
        }

        public T LastElement<T>()
        {
            ValidateSupportedType<T>();

            string element = this.Last();

            return StringConverter.Convert<T>(element);
        }

        public List<T> ToList<T>()
        {
            List<T> list = new List<T>();

            while (!Empty)
            {
                list.Add(NextElement<T>());
            }

            return list;
        }

        public string ToSingleString(string wordSeparator = " ")
        {
            StringBuilder stringBuilder = new StringBuilder();

            while (!Empty)
            {
                stringBuilder.Append(NextElement<string>());
                if (!Empty)
                {
                    stringBuilder.Append(wordSeparator);
                }
            }

            return stringBuilder.ToString();
        }

        public void Append(string str) => Enqueue(str);

        #region Private methods

        /// <summary>
        /// Returns next element of a ParsedLine, converting it to T and removing it from the Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T Extract<T>()
        {
            ValidateSupportedType<T>();

            string stringToConvert = Dequeue();

            if (typeof(T) == typeof(string))
            {
                return (T)(object)stringToConvert;
            }
            else if (typeof(T) == typeof(char))
            {
                if (this.Any())
                {
                    throw new NotSupportedException("Extract<char> can only be used with one-length Queues" +
                       " Try using ExtractChar<string> instead, after parsing each string with Extract<string>()");
                }

                char nextChar = ExtractChar(ref stringToConvert);
                if (stringToConvert.Length > 0)
                {
                    Enqueue(stringToConvert);
                }

                return (T)(object)nextChar;
            }

            return StringConverter.Convert<T>(stringToConvert);
        }

        /// <summary>
        /// Returns next element of a ParsedLine, converting it to T but WITHOUT removing it from the Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T Peek<T>()
        {
            ValidateSupportedType<T>();

            string stringToConvert = Peek();

            return StringConverter.Convert<T>(stringToConvert);
        }

        private static void ValidateSupportedType<T>()
        {
            if (!SupportedTypes.Contains(typeof(T)))
            {
                throw new NotSupportedException("Parsing to " + typeof(T).ToString() + "is not supported yet");
            }
        }

        /// <summary>
        /// Returns next char of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static char ExtractChar(ref string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ParsingException("String is empty");
            }

            char nextChar = str[0];
            str = str.Substring(1);

            return nextChar;
        }

        /// <summary>
        /// Supported parsing conversions
        /// </summary>
        private static HashSet<Type> SupportedTypes { get; } = new HashSet<Type>()
        {
            typeof(bool),
            typeof(char),
            typeof(string),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(double),
            typeof(object)
        };

        #endregion
    }
}
