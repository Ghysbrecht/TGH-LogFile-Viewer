using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    class LogFileFactory
    {
        public List<LogFile> getLogFilesFromJson(String json)
        {
            List<LogFile> logFiles = new List<LogFile>();

            if (json != "")
            {
                var jsonObj = JObject.Parse(json);
                if (jsonObj["error"] == null)
                {
                    foreach (var file in jsonObj["aggregations"]["filename"]["buckets"])
                    {
                        LogFile logfile = new LogFile((String)file["key"], (int)file["doc_count"]);
                        logFiles.Add(logfile);
                    }

                    return logFiles;
                }
                else
                {
                    Console.WriteLine("ERROR - Returned json contains an error message!");
                }
            }
            else
            {
                Console.WriteLine("ERROR - Returned json is empty!");
            }

            return logFiles;

        }
    }
}
