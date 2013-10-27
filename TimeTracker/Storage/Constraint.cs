using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage
{
    interface IConstraint
    {
        string GenerateAsColumnConstraint(string columnName);

        string GenerateAsTableConstraint(ISet<string> columnNames);
    }

    abstract class Constraint<T> : IConstraint where T : Attribute
    {
        public bool Matched { get; protected set; }

        public readonly T Attribute;

        protected Constraint(T attribute)
        {
            Attribute = attribute;
            Matched = false;

            Validate();
        }

        public abstract string GenerateAsColumnConstraint(string columnName);

        public abstract string GenerateAsTableConstraint(ISet<string> columnNames);

        protected abstract void Validate();
    }
}
