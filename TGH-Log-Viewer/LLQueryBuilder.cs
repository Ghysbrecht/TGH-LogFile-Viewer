using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace TGH_Log_Viewer
{
    class LLQueryBuilder
    {
        ElasticLowLevelClient client;
        LogLineFactory logLineFactory = new LogLineFactory();

        String savedTerm, lastExecuted, mainIndex;
        DateTime leftBound, rightBound;
        long lastHits = 0;

        public LLQueryBuilder(ElasticLowLevelClient client)
        {
            setClient(client);
            setTimeBoundsDefault();
            mainIndex = "maintest";
        }

        public void setClient(ElasticLowLevelClient client)
        {
            this.client = client;
        }

        public void setMainIndex(String index)
        {
            this.mainIndex = index;
        }

        public List<LogLine> getAllData(int offset, int records)
        {
            return toLogLines(filterOn("all", "", offset, records));
        }

        public List<LogLine> filterOnFilename(int offset, int records, String term)
        {
            return toLogLines(filterOn("filename", term, offset, records));
        }

        public List<LogLine> filterOnFunction(int offset, int records, String term)
        {
            return toLogLines(filterOn("function", term, offset, records));
        }

        public List<LogLine> filterOnProcess(int offset, int records, String term)
        {
            return toLogLines(filterOn("process", term, offset, records));
        }

        public List<LogLine> filterOnPID(int offset, int records, String term)
        {
            return toLogLines(filterOn("PID", term, offset, records));
        }

        public List<LogLine> filterOnTID(int offset, int records, String term)
        {
            return toLogLines(filterOn("TID", term, offset, records));
        }

        public List<LogLine> filterOnLoglevel(int offset, int records, String term)
        {
            return toLogLines(filterOn("loglevel", term, offset, records));
        }

        public List<LogLine> filterOnLogtype(int offset, int records, String term)
        {
            return toLogLines(filterOn("logtype", term, offset, records));
        }

        public List<LogLine> filterOnMessage(int offset, int records, String term)
        {
            return toLogLines(filterOn("messagedata", term, offset, records));
        }

        private String filterOn(String column, String message, int offset, int records)
        {
            lastExecuted = column;
            savedTerm = message;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\"from\":" + offset + ",\"size\":" + records + ","); //From & Size
            stringBuilder.Append("\"sort\":[{\"@timestamp\":{\"order\":\"asc\"}}],");   //Order by timestamp ascending
            stringBuilder.Append("\"query\":{\"bool\":{\"must\":[");

            if (column == "all") stringBuilder.Append("{\"match_all\":{}}");
            else if (column == "global") stringBuilder.Append("{\"multi_match\": {\"query\": \" "+ message + "\"}}");
            else stringBuilder.Append("{\"match_phrase\":{\"" + column + "\":\"" + message + "\"}}"); //Match Phrase
                 //stringBuilder.Append(",");
            stringBuilder.Append("],\"filter\":[{\"range\":{\"@timestamp\":{");
            stringBuilder.Append("\"gte\":\"" + leftBound.Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalMilliseconds + "\",");
            stringBuilder.Append("\"lt\":\"" + rightBound.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + "\"}}}]}}}");

            var searchResponse = client.Search<StringResponse>(mainIndex, stringBuilder.ToString());
            return searchResponse.Body;
        }

        public List<LogLine> globalSearch(String term, int offset, int records)
        {
            return toLogLines(filterOn("global", term, offset, records));
        }

        private List<LogLine> toLogLines(String json)
        {
            ParsedLogLines parsedLines = logLineFactory.getLogLinesFromJson(json);
            lastHits = parsedLines.hits;
            return parsedLines.loglines;
        }

        //Get the total ammount of hits of the last executed query
        public long getLastResponseHits()
        {
            return lastHits;
        }

        public List<LogLine> lastQueryNewPage(int offset, int records)
        {
            if (lastExecuted == "all") return getAllData(offset, records);
            else if (lastExecuted == "global") return globalSearch(savedTerm, offset, records);
            else return toLogLines(filterOn(lastExecuted, savedTerm, offset, records));
        }

        public void setTimeBounds(DateTime leftBound, DateTime rightBound)
        {
            this.leftBound = leftBound;
            this.rightBound = rightBound;
        }

        public void setTimeBoundsDefault()
        {
            setTimeBounds(DateTime.Now.AddYears(-5), DateTime.Now);
        }
    }
}
