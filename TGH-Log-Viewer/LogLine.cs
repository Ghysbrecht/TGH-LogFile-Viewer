using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    class LogLine
    {
        public int PID { get; set; }
        public int TID { get; set; }
        public string filename { get; set; }
        public string function { get; set; }
        public string loglevel { get; set; }
        public string logtype { get; set; }
        public string messagedata { get; set; }
        public string process { get; set; }
        public DateTime timestamp { get; set; }
    }
}
