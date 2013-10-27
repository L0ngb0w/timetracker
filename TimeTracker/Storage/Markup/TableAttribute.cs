using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string Name { get; private set; }

        public bool IfNotExists { get; set; }

        public TableAttribute(string name)
        {
            Name = name;

            IfNotExists = false;
        }

        public TableAttribute()
            : this(string.Empty)
        {
        }
    }
}
