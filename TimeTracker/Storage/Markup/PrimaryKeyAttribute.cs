using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        public IEnumerable<string> Columns { get; private set; }

        public string Name { get; set; }

        public bool AutoIncrement { get; set; }

        public PrimaryKeyAttribute(params string[] columns)
        {
            Columns = columns;

            Name = null;
            AutoIncrement = false;
        }
    }
}
