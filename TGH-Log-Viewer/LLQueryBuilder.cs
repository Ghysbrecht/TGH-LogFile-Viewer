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
        SuggestionFactory suggestionFactory = new SuggestionFactory();
        Logger logger = new Logger();

        String savedTerm, lastExecuted, mainIndex;
        DateTime leftBound, rightBound;
        long lastHits = 0;
        String lastError = "";

        List<SearchRequest> searchHistory;


        public LLQueryBuilder(ElasticLowLevelClient client)
        {
            setClient(client);
            setTimeBoundsDefault();
            mainIndex = "maintest";
            setElasticSettings();
            logger.debug("QueryBuilder created!");
            searchHistory = new List<SearchRequest>();
        }

        public void setClient(ElasticLowLevelClient client)
        {
            this.client = client;
            setElasticSettings();
        }

        public void setMainIndex(String index)
        {
            mainIndex = index;
        }

        public List<LogLine> getAllData(int offset, int records, bool saveInHistory = true)
        {
            return toLogLines(filterOn("all", "", offset, records, saveInHistory));
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
            term = term.Replace('\r', ' ');
            term = term.Replace(@"\", @"\\");
            return toLogLines(filterOn("messagedata", term, offset, records));
        }

        private String filterOn(String column, String message, int offset, int records, bool saveInHistory = true)
        {
            if(saveInHistory) searchHistory.Add(new SearchRequest(message, column));
            logger.debug("Filtering on: OF:" + offset + " RE:" + records + " COL:" + column + " -> " + (message.Length <= 50 ? message : message.Substring(0,50)));
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
            stringBuilder.Append("\"gte\":\"" + Math.Round(leftBound.Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalMilliseconds,0)  + "\",");
            stringBuilder.Append("\"lt\":\"" + Math.Round(rightBound.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,0)  + "\"}}}]}}}");

            return executeQuery(mainIndex, stringBuilder.ToString());
        }

        public List<String> getSuggestionsFor(String column, String text)
        {
            logger.debug("Getting suggestions for: " + column + " -> " + text);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\"size\":0,\"aggs\":{\"logtypes\":{\"terms\":{\"field\":\"" + column + ".keyword\",\"size\":5,\"include\":\"" + text + ".*\"}}}}");

            return toSuggestions(executeQuery(mainIndex, stringBuilder.ToString()));
        }

        private List<String> toSuggestions(String json)
        {
            return suggestionFactory.getSuggestionsFromJson(json);
        }

        public List<LogLine> globalSearch(String term, int offset, int records, bool saveInHistory = true)
        {
            return toLogLines(filterOn("global", term, offset, records,saveInHistory));
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
            return requeryPrevious(0, offset, records);
        }

        public List<LogLine> previousQuery(int offset, int records)
        {
            if(searchHistory.Count > 1) searchHistory.Remove(searchHistory.Last());
            return lastQueryNewPage(offset, records);
        }

        public SearchRequest getLastQueryData()
        {
            return (searchHistory.Count > 0) ? searchHistory.Last() : new SearchRequest("", "");
        }


        //Requery a query that was once executed 0 = most recent, 1 = that one before that etc.
        public List<LogLine> requeryPrevious(int historyIndex,int offset, int records)
        {
            Console.WriteLine("Requested previous query: historyIndex -> " + historyIndex + "   Ammount of history objects: " + searchHistory.Count );
            if(historyIndex < 0 || historyIndex >= searchHistory.Count)
            {
                logger.error("RequeryPrevious index out of range!");
                historyIndex = 0;
            }
            int elementNumber = searchHistory.Count - (historyIndex + 1);
            if (elementNumber >= 0 && elementNumber < searchHistory.Count)
            {
                if (historyIndex == 0) return reExecute(searchHistory.ElementAt(elementNumber), offset, records, false);
                else return reExecute(searchHistory.ElementAt(elementNumber), offset, records);
            }
            else
            {
                logger.error("RequeryPrevious - No elements in history!");
                return null;
            }
        }

        private List<LogLine> reExecute(SearchRequest request, int offset, int records, bool saveInHistory = true)
        {
            if (request.searchColumn == "all") return getAllData(offset, records, saveInHistory);
            else if (request.searchColumn == "global") return globalSearch(request.searchTerm, offset, records, saveInHistory);
            else return toLogLines(filterOn(request.searchColumn, request.searchTerm, offset, records, saveInHistory));
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

        private void setElasticSettings()
        {
            logger.debug("Setting elastic settings");
            var request = client.IndicesPutSettingsForAll<StringResponse>("{\"max_result_window\":1000000}");
        }

        private String executeQuery(String index, String query)
        {
            try
            {
                var searchResponse = client.Search<StringResponse>(index, query);
                lastError = "";
                return searchResponse.Body;
            }
            catch(Exception e)
            {
                logger.error("Exception when trying to execute a query! -> " + e.Message);
                lastError = e.Message;
                return "";
            }
        }

        public String getLastError()
        {
            return lastError;
        }
    }
}
