using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }

        public TypeAffinity TypeAffinity { get; set; }

        public bool NotNull { get; set; }

        public bool Unique { get; set; }

        public string Default { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;

            TypeAffinity = TypeAffinity.None;
            NotNull = false;
            Unique = false;
        }

        public ColumnAttribute()
            : this(string.Empty)
        {
        }
    }
}
