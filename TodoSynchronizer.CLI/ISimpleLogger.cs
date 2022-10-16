using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.CLI
{
    public interface ISimpleLogger
    {
        void Log(string message);
    }
}
