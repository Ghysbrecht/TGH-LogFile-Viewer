using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TGH_Log_Viewer
{
    public class LogLineFactory
    {
        public ParsedLogLines getLogLinesFromJson(String json)
        {
            List<LogLine> logLineCollection = new List<LogLine>();


            var jsonObj = JObject.Parse(json);

            if (jsonObj["error"] == null)
            {
                foreach (var hit in jsonObj["hits"]["hits"])
                {
                    LogLine logline = new LogLine();
                    logline.timestamp = (DateTime)hit["_source"]["@timestamp"];
                    logline.PID = (String)hit["_source"]["PID"];
                    logline.TID = (String)hit["_source"]["TID"];
                    logline.messagedata = (String)hit["_source"]["messagedata"];
                    logline.filename = (String)hit["_source"]["filename"];
                    logline.process = (String)hit["_source"]["process"];
                    logline.function = (String)hit["_source"]["function"];
                    logline.loglevel = (String)hit["_source"]["loglevel"];
                    logline.logtype = (String)hit["_source"]["logtype"];

                    logLineCollection.Add(logline);
                }

                ParsedLogLines parsedLogLines = new ParsedLogLines();
                parsedLogLines.loglines = logLineCollection;
                parsedLogLines.hits = (long)jsonObj["hits"]["total"];


                return parsedLogLines;
            }
            else
            {
                Console.WriteLine("ERROR - Returned json contains an error message!");
                return new ParsedLogLines();
            }

               
        }
    }
}
