using System;
using System.Collections.Generic;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Text;

namespace Json.Embed
{
  internal class JsonReaderException : Exception
  {
    public int LineNumber { get; }
    public int LinePosition { get; }

    public JsonReaderException() : base()
    {
    }

#if SERIALIZATION
    protected JsonReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif

    public JsonReaderException(JsonTextReader reader, string message) : base(message)
    {
      LineNumber = reader.LineNumber;
      LinePosition = reader.LinePosition;
    }

    public JsonReaderException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}
