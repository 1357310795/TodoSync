using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.CLI
{
    public class ConsoleAdapter : ISimpleLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
