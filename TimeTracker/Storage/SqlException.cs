using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage
{
    public class SqlException : Exception
    {
        readonly string mStatement;

        public string Statement { get { return mStatement; } }

        public SqlException()
            : base()
        {
        }

        public SqlException(string message, string statement)
            : base(message)
        {
            mStatement = statement;
        }

        public SqlException(string message, string statement, Exception innerException)
            : base(message, innerException)
        {
            mStatement = statement;
        }

        public SqlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
