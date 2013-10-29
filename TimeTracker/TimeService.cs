using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    class TimeService
    {
        public static DateTime Time
        {
            get
            {
                var now = DateTime.Now;
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            }
        }

        public static DateTime Date
        {
            get { return DateTime.Today; }
        }
    }
}
