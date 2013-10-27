using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public IEnumerable<string> Columns { get; private set; }

        public string Name { get; set; }

        public bool Unique { get; set; }

        public bool IfNotExists { get; set; }

        public IndexAttribute(params string[] columns)
        {
            Columns = columns;

            Name = string.Empty;
            Unique = false;
            IfNotExists = false;
        }
    }
}
