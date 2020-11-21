using System;
using System.Runtime.Serialization;

namespace FileParser
{
    [Serializable]
    public class ParsingException : Exception
    {
        private const string _genericMessage = "Exception triggered during parsing process";

        public ParsingException() : base(_genericMessage)
        {
        }

        public ParsingException(string message) : base(message ?? _genericMessage)
        {
        }

        public ParsingException(string message, Exception inner) : base(message ?? _genericMessage, inner)
        {
        }

        protected ParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
