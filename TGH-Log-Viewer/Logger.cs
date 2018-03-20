using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

/*
    This loggerclass is written by Toby Patke, downloaded from codeproject.com.
*/

namespace TGH_Log_Viewer
{
    public class Logger
    {
        bool logToConsole;
        String path;

        public Logger(bool logToConsole = true, String path = null)
        {
            if (path == null) this.path = System.Reflection.Assembly.GetEntryAssembly().Location + ".log.txt";
            else this.path = path;
            this.logToConsole = logToConsole;

        }

        public void debug(String message)
        {
            logLine("[DBG] - " + message);
        }
        
        public void error(String message)
        {
            logLine("[ERR] - " + message);
        }

        private void logLine(String message)
        {
            message = (DateTime.Now).ToShortDateString() + " " + (DateTime.Now).ToLongTimeString() + " - " + message;
            if (logToConsole) Console.WriteLine(message);
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(message);
            }
        }
    }
}
