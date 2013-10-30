using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage.Markup;

namespace TimeTracker.Tables
{
    [Table(IfNotExists = true)]
    class Configuration
    {
        [Column(NotNull = true, TypeAffinity = TypeAffinity.Real)]
        public double WindowWidth;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Real)]
        public double WindowHeight;

        [Column(NotNull = true, Default = "0", TypeAffinity = TypeAffinity.Real)]
        public double WindowX;

        [Column(NotNull = true, Default = "0", TypeAffinity = TypeAffinity.Real)]
        public double WindowY;
    }
}
