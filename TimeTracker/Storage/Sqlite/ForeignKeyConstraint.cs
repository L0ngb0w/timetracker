using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage.Markup;

namespace TimeTracker.Storage.Sqlite
{
    class ForeignKeyConstraint : Constraint<ForeignKeyAttribute>
    {
        public ForeignKeyConstraint(ForeignKeyAttribute foreignKey)
            : base(foreignKey)
        {
        }

        public override string GenerateAsColumnConstraint(string columnName)
        {
            if (Attribute.Columns.Count() != 1 || Attribute.Columns.First() != columnName)
                return null;

            var clause = new StringBuilder();

            GenerateConstraintClause(clause);
            GenerateForeignKeyClause(clause);
            GenerateReferenceActionClause(clause);

            Matched = true;
            return clause.ToString();
        }

        public override string GenerateAsTableConstraint(ISet<string> columnNames)
        {
            if (Attribute.Columns.Count() < 2)
                return null;

            foreach (var columnName in Attribute.Columns.Where(name => !columnNames.Contains(name)))
                throw new MarkupException("ForeignKey", "Foreign key columns did not match the table columns");

            var clause = new StringBuilder();

            GenerateConstraintClause(clause);

            clause.
                Append("FOREIGN KEY (").
                Append(string.Join(",", Attribute.Columns)).
                Append(") ");

            GenerateForeignKeyClause(clause);
            GenerateReferenceActionClause(clause);

            Matched = true;
            return clause.ToString();
        }

        protected override void Validate()
        {
            if (Attribute.Columns == null || !Attribute.Columns.Any())
                throw new MarkupException("ForeignKey", "Foreign key must have at least one column");

            if (string.IsNullOrEmpty(Attribute.ForeignTable))
                throw new MarkupException("ForeignKey", "Foreign key must reference a foreign table");

            if (Attribute.ForeignColumns != null && Attribute.ForeignColumns.Any() && Attribute.Columns.Count() != Attribute.ForeignColumns.Count())
                throw new MarkupException("ForeignKey", "Foreign key must have the same number of foreign key and local columns");
        }

        private void GenerateConstraintClause(StringBuilder clause)
        {
            if (!string.IsNullOrEmpty(Attribute.Name))
                clause.Append("CONSTRAINT ").Append(Attribute.Name).Append(" ");
        }

        void GenerateForeignKeyClause(StringBuilder clause)
        {
            clause.Append("REFERENCES ").Append(Attribute.ForeignTable);

            if (Attribute.ForeignColumns != null && Attribute.ForeignColumns.Any())
                clause.Append(" (").Append(string.Join(",", Attribute.ForeignColumns)).Append(")");
        }

        void GenerateReferenceActionClause(StringBuilder clause)
        {
            GenerateOnDeleteReferenceActionClause(clause);
            GenerateOnUpdateReferenceActionClause(clause);
        }

        void GenerateOnDeleteReferenceActionClause(StringBuilder clause)
        {
            if (Attribute.OnDelete != ReferenceAction.NoAction)
            {
                clause.Append(" ON DELETE ").Append(GenerateReferenceAction(Attribute.OnDelete));
            }
        }

        void GenerateOnUpdateReferenceActionClause(StringBuilder clause)
        {
            if (Attribute.OnUpdate != ReferenceAction.NoAction)
            {
                clause.Append(" ON UPDATE ").Append(GenerateReferenceAction(Attribute.OnUpdate));
            }
        }

        static string GenerateReferenceAction(ReferenceAction action)
        {
            switch (action)
            {
                case Markup.ReferenceAction.Cascade:
                    return "CASCADE";
                case Markup.ReferenceAction.NoAction:
                    return "NO ACTION";
                case Markup.ReferenceAction.Restrict:
                    return "RESTRICT";
                case Markup.ReferenceAction.SetDefault:
                    return "SET DEFAULT";
                case Markup.ReferenceAction.SetNull:
                    return "SET NULL";
                default:
                    throw new Exception("Unsupported reference action: " + action);
            }
        }
    }
}
