using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage
{
    public enum TransactionBehavior
    {
        Deferred,
        Immediate,
        Exclusive,
    }

    public interface ITransaction : IDisposable
    {
        void Rollback();

        void Commit();
    }
}
