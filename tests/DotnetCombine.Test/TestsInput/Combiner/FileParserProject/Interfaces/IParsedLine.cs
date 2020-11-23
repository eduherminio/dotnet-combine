using System.Collections.Generic;

namespace FileParser
{
    public interface IParsedLine
    {
        /// <summary>
        /// Returns the size (number of elements) of ParsedLine
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns whether ParsedLine has no elements
        /// </summary>
        bool Empty { get; }

        /// <summary>
        /// Returns next element of type T, removing it from ParsedLine
        /// </summary>
        /// <exception cref="ParsingException">Line is already empty</exception>
        /// <exception cref="System.NotSupportedException">Parsing to chosen type is not supported yet or T is char and line's length > 1</exception>
        /// <returns>Next element</returns>
        T NextElement<T>();

        /// <summary>
        /// Returns next element of type T, not modifying ParsedLine.
        /// This still allows its modification
        /// </summary>
        /// <exception cref="ParsingException">Line is already empty</exception>
        /// <exception cref="System.NotSupportedException">Parsing to chosen type is not supported yet</exception>
        /// <returns>Next element</returns>
        T PeekNextElement<T>();

        /// <summary>
        /// Returns element at a specified index, allowing its modification
        /// </summary>
        /// <param name="index">zero-based index</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Element at the specified position in line</returns>
        /// <exception cref="System.NotSupportedException">Parsing to selected type is not supported yet</exception>
        /// <exception cref="System.ArgumentNullException">File is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than 0 or greater than or equal to the number of lines in file</exception>
        T ElementAt<T>(int index);

        /// <summary>
        /// Returns last element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Last element</returns>
        /// <exception cref="System.NotSupportedException">Parsing to selected type is not supported yet</exception>
        /// <exception cref="System.ArgumentNullException">Line is null</exception>
        /// <exception cref="System.InvalidOperationException">Line is empty</exception>
        T LastElement<T>();

        /// <summary>
        /// Returns remaining elements as a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> ToList<T>();

        /// <summary>
        /// Returns remaining elements as a single string, separated by wordSeparator
        /// </summary>
        /// <param name="wordSeparator"></param>
        /// <returns></returns>
        string ToSingleString(string wordSeparator = " ");

        /// <summary>
        /// Appends a string to the end of the line
        /// </summary>
        /// <param name="str"></param>
        void Append(string str);
    }
}
