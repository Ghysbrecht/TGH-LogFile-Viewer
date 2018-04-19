using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace TGH_Log_Viewer
{
    public class LLQueryBuilder
    {
        ElasticLowLevelClient client;
        LogLineFactory logLineFactory = new LogLineFactory();
        SuggestionFactory suggestionFactory = new SuggestionFactory();
        LogFileFactory logFileFactory = new LogFileFactory();
        Logger logger = new Logger();

        String  mainIndex;
        DateTime leftBound, rightBound;
        long lastHits = 0;
        String lastError = "";

        List<FileExclusion> fileExclusions;
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

        //--- SETTER & GETTERS ----
        //Set the client used by this class to execute the queries
        public void setClient(ElasticLowLevelClient client)
        {
            this.client = client;
            setElasticSettings();
        }
        //Set the index that is used for all queries
        public void setMainIndex(String index)
        {
            mainIndex = index;
        }
        //Get the total ammount of hits of the last executed query
        public long getLastResponseHits()
        {
            return lastHits;
        }
        //Set the timebounds for time filtering
        public void setTimeBounds(DateTime leftBound, DateTime rightBound)
        {
            this.leftBound = leftBound;
            this.rightBound = rightBound;
        }
        //Set the timebounds to a default value
        public void setTimeBoundsDefault()
        {
            setTimeBounds(DateTime.Now.AddYears(-5), DateTime.Now);
        }
        //Get the most recent error
        public String getLastError()
        {
            return lastError;
        }
        //Set the fileExclusion list
        public void setFileExclusions(List<FileExclusion> list)
        {
            fileExclusions = list;
        }


        //--- FILTERS ---
        //Get all data without filters
        public List<LogLine> getAllData(int offset, int records, bool saveInHistory = true)
        {
            return toLogLines(filterOn(new SearchRequest("", "all", leftBound, rightBound, offset, records)));
        }
        //Filter on FILENAME
        public List<LogLine> filterOnFilename(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "filename", leftBound, rightBound, offset, records)));
        }
        //Filter on FUNCTION
        public List<LogLine> filterOnFunction(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "function", leftBound, rightBound, offset, records)));
        }
        //Filter on PROCESS
        public List<LogLine> filterOnProcess(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "process", leftBound, rightBound, offset, records)));
        }
        //Filter on PID
        public List<LogLine> filterOnPID(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "PID", leftBound, rightBound, offset, records)));
        }
        //Filter on TID
        public List<LogLine> filterOnTID(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "TID", leftBound, rightBound, offset, records)));
        }
        //Filter on LOGLEVEL
        public List<LogLine> filterOnLoglevel(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "loglevel", leftBound, rightBound, offset, records)));
        }
        //Filter on LOGTYPE
        public List<LogLine> filterOnLogtype(int offset, int records, String term)
        {
            return toLogLines(filterOn(new SearchRequest(term, "logtype", leftBound, rightBound, offset, records)));
        }
        //Filter on MESSAGE
        public List<LogLine> filterOnMessage(int offset, int records, String term)
        {
            term = term.Replace('\r', ' ');
            term = term.Replace(@"\", @"\\");
            term = term.Replace('\n', ' ');
            return toLogLines(filterOn(new SearchRequest(term, "messagedata", leftBound, rightBound, offset, records)));
        }
        //Filter on ALL (Global Search)
        public List<LogLine> globalSearch(String term, int offset, int records, bool saveInHistory = true)
        {
            return toLogLines(filterOn(new SearchRequest(term, "global", leftBound, rightBound, offset, records), saveInHistory));
        }
        //Main FILTER ON method used by all the other ones
        private String filterOn(SearchRequest request, bool saveInHistory = true)
        {
            if(saveInHistory) searchHistory.Add(request);
            //historyDebug();
            leftBound = request.startDate;
            rightBound = request.endDate;
            logger.debug("Filtering on: OF:" + request.offset + " RE:" + request.records + " COL:" + request.searchColumn + " -> " + (request.searchTerm.Length <= 50 ? request.searchTerm : request.searchTerm.Substring(0,50)));

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\"from\":" + request.offset + ",\"size\":" + request.records + ","); //From & Size
            stringBuilder.Append("\"sort\":[{\"@timestamp\":{\"order\":\"asc\"}}],");   //Order by timestamp ascending
            stringBuilder.Append("\"query\":{\"bool\":{\"must\":[");
            //MUST
            if (request.searchColumn == "all") stringBuilder.Append("{\"match_all\":{}}");
            else if (request.searchColumn == "global") stringBuilder.Append("{\"multi_match\": {\"query\": \" "+ request.searchTerm + "\"}}");
            else stringBuilder.Append("{\"match_phrase\":{\"" + request.searchColumn + "\":\"" + request.searchTerm + "\"}}"); //Match Phrase

            stringBuilder.Append("],\"must_not\":[");
            //MUST NOT
            appendFileExlusions(fileExclusions, stringBuilder);

            stringBuilder.Append("],\"filter\":[{\"range\":{\"@timestamp\":{");
            stringBuilder.Append("\"gte\":\"" + Math.Round(request.startDate.Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalMilliseconds,0)  + "\",");
            stringBuilder.Append("\"lt\":\"" + Math.Round(request.endDate.Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalMilliseconds,0)  + "\"}}}]}}}");

            return executeQuery(mainIndex, stringBuilder.ToString());
        }


        //--- SUGGESTIONS ---
        //Get suggestions for a column
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

        
        //--- GENERAL ---
        //Convert the JSON to a List of LogLines
        private List<LogLine> toLogLines(String json)
        {
            ParsedLogLines parsedLines = logLineFactory.getLogLinesFromJson(json);
            lastHits = parsedLines.hits;
            return parsedLines.loglines;
        }
        //Set the elasticsearch settings to allow scrolling
        private void setElasticSettings()
        {
            logger.debug("Setting elastic settings");
            var request = client.IndicesPutSettingsForAll<StringResponse>("{\"max_result_window\":1000000}");
        }
        //Execute the query
        private String executeQuery(String index, String query)
        {
            try
            {
                var searchResponse = client.Search<StringResponse>(index, query);
                lastError = "";
                return searchResponse.Body;
            }
            catch (Exception e)
            {
                logger.error("Exception when trying to execute a query! -> " + e.Message);
                lastError = e.Message;
                return "";
            }
        }


        //--- HISTORY ---
        //Requery the last used query with new page parameters
        public List<LogLine> lastQueryNewPage(int offset, int records, bool useGlobalTime = false)
        {
            SearchRequest lastReq = getLastQueryData().cloneThis();
            lastReq.offset = offset;
            lastReq.records = records;
            if (useGlobalTime)
            {
                lastReq.startDate = leftBound;
                lastReq.endDate = rightBound;
            }
            return toLogLines(filterOn(lastReq));
        }
        //Requery the last used query with new date parameters
        public List<LogLine> lastQueryNewDates(DateTime leftBound, DateTime rightBound, int offset, int records)
        {
            SearchRequest lastReq = getLastQueryData().cloneThis();
            lastReq.startDate = leftBound;
            lastReq.endDate = rightBound;
            lastReq.offset = offset;
            lastReq.records = records;
            return toLogLines(filterOn(lastReq));
        }
        //Go back to the previous query by removing the latest
        public List<LogLine> previousQuery()
        {
            if(searchHistory.Count > 1) searchHistory.Remove(searchHistory.Last());
            return lastQuery();
        }
        public List<LogLine> lastQuery()
        {
            return toLogLines(filterOn(getLastQueryData(), false));
        }
        //Get the most recent searchQuery
        public SearchRequest getLastQueryData()
        {
            return (searchHistory.Count > 0) ? searchHistory.Last() : new SearchRequest("", "", new DateTime(), new DateTime(),0,100);
        }

        //Print the current history
        public void historyDebug()
        {
            Console.WriteLine("********* PRINTING HISTORY ************");
            foreach(SearchRequest search in searchHistory)
            {
                Console.WriteLine(searchHistory.IndexOf(search).ToString().PadRight(3) + " : >COL: " + search.searchColumn.PadRight(20) + " >DATA: " + search.searchTerm.PadRight(20) + " >START: " + search.startDate.ToString("yyy-MM-dd HH:mm:ss.fff") + " >END: " + search.endDate.ToString("yyy-MM-dd HH:mm:ss.fff") + ">OFFSET: " + search.offset.ToString().PadRight(7) + ">RECORDS: " + search.records);
            }
            Console.WriteLine("********* END OF HISTORY ************");
        }


        //--- EXCLUSIONS ---
        //Query the database to retrieve a list of the included filenames
        public List<LogFile> getLogFiles(List<FileExclusion> exclusions = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\"size\":0,\"query\":{\"bool\":{\"must_not\":[");
            appendFileExlusions(exclusions, stringBuilder);
            stringBuilder.Append("]}},\"aggs\":{\"filename\":{\"terms\":{\"field\":\"filename.keyword\",\"size\":1000}}}}");
                

            return toLogFiles(executeQuery(mainIndex, stringBuilder.ToString()));
        }
        //Convert the JSON to a list of LogFile objects
        private List<LogFile> toLogFiles(String json)
        {
            return logFileFactory.getLogFilesFromJson(json);
        }

        private void appendFileExlusions(List<FileExclusion> exclusions, StringBuilder stringBuilder)
        {
            if (exclusions != null && exclusions.Count > 0)
            {
                FileExclusion last = exclusions.Last();
                foreach (FileExclusion exclusion in exclusions)
                {
                    switch (exclusion.type)
                    {
                        case Resources.IEnums.ExclusionType.Term: stringBuilder.Append("{\"match_phrase\": {\"filename\": \"" + exclusion.rule + "\"}}"); break;
                        case Resources.IEnums.ExclusionType.Wildcard: stringBuilder.Append("{\"wildcard\": {\"filename.keyword\": \"" + exclusion.rule + "\"}}"); break;
                    }
                    if (exclusion != last) stringBuilder.Append(",");
                }
            }
        }

    }
}
