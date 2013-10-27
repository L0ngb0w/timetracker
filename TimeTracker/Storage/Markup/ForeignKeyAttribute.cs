using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Markup
{
    public enum ReferenceAction
    {
        SetNull,
        SetDefault,
        Cascade,
        Restrict,
        NoAction,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class ForeignKeyAttribute : Attribute
    {
        public string Name { get; set; }

        public string[] Columns { get; set; }

        public string ForeignTable { get; set; }

        public string[] ForeignColumns { get; set; }

        public ReferenceAction OnDelete { get; set; }

        public ReferenceAction OnUpdate { get; set; }

        public ForeignKeyAttribute()
        {
            Name = null;
            Columns = null;
            ForeignTable = null;
            ForeignColumns = null;
            OnDelete = ReferenceAction.NoAction;
            OnUpdate = ReferenceAction.NoAction;
        }
    }
}
