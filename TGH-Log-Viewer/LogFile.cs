using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    public class LogFile
    {
        public LogFile(String fileName, int count)
        {
            this.fileName = fileName;
            this.count = count;
        }
        public String fileName { get; set; }
        public int count { get; set; }
    }
}
