using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    public class MarkupException : Exception
    {
        public string Attribute { get; private set; }

        public MarkupException()
            : base()
        {
        }

        public MarkupException(string attribute)
            : base()
        {
            Attribute = attribute;
        }

        public MarkupException(string attribute, string message)
            : base(message)
        {
            Attribute = attribute;
        }

        public MarkupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MarkupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
