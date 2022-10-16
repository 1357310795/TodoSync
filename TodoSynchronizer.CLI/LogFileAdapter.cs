using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.CLI
{
    public class LogFileAdapter : ISimpleLogger
    {
        FileStream fs;
        StreamWriter sw;
        public LogFileAdapter(string filename) 
        {
            fs = new FileStream(filename, FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs);
        }
        public void Log(string message)
        {
            sw.WriteLine(message);
            sw.Flush();
            fs.Flush();
        }

        ~LogFileAdapter() { sw.Close();fs.Close(); }
    }
}
