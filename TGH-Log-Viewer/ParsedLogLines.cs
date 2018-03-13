using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    public class ParsedLogLines
    {
        public long hits { get; set; }
        public List<LogLine> loglines { get; set; }
    }
}
