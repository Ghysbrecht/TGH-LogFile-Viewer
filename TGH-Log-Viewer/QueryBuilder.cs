using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace TGH_Log_Viewer
{
    class QueryBuilder
    {
        Object lastResponse;
        String lastExecuted;
        ElasticClient client;

        String savedFilename, savedFunctionName, savedProcessName, savedPID, savedTID, savedLoglevel, savedLogtype, savedMessage;

        DateTime leftBound, rightBound;

        public QueryBuilder(ElasticClient client)
        {
            setClient(client);
            setTimeBoundsDefault();
        }

        public void setClient(ElasticClient client)
        {
            this.client = client;
        }

        //Get all the data in the main index where field = X and logtype = Y
        public IReadOnlyCollection<LogLine> getSpecificData(int offset, int records)
        {
            lastExecuted = "getSpecificData";
            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.filename)
                                .Query("SLElement")
                            )//, m => m
                            //.MatchPhrase(c => c
                            //    .Field(p => p.logtype)
                            //    .Query("DBG")
                            //)
                        )
                    )
                )
            );


            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        //Search queries
        public IReadOnlyCollection<LogLine> getAllData(int offset, int records)
        {
            lastExecuted = "getAllData";
            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchAll()
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );
            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnFilename(int offset, int records, String filename)
        {
            lastExecuted = "filterOnFilename";
            savedFilename = filename;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.filename)
                                .Query(filename)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnFunction(int offset, int records, String functionName)
        {
            lastExecuted = "filterOnFunction";
            savedFunctionName = functionName;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.function)
                                .Query(functionName)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnProcess(int offset, int records, String processName)
        {
            lastExecuted = "filterOnProcess";
            savedProcessName = processName;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.process)
                                .Query(processName)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnPID(int offset, int records, String PIDstr)
        {
            lastExecuted = "filterOnPID";
            savedPID = PIDstr;
            

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.PID)
                                .Query(PIDstr)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnTID(int offset, int records, String TIDstr)
        {
            lastExecuted = "filterOnTID";
            savedTID = TIDstr;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.TID)
                                .Query(TIDstr)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnLoglevel(int offset, int records, String loglevel)
        {
            lastExecuted = "filterOnLoglevel";
            savedLoglevel = loglevel;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.loglevel)
                                .Query(loglevel)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnLogtype(int offset, int records, String logtype)
        {
            lastExecuted = "filterOnLogtype";
            savedLogtype = logtype;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.logtype)
                                .Query(logtype)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        public IReadOnlyCollection<LogLine> filterOnMessage(int offset, int records, String message)
        {
            lastExecuted = "filterOnMessage";
            savedMessage = message;

            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Sort(ss => ss
                    .Ascending(p =>
                        p.timestamp
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.messagedata)
                                .Query(message)
                            )
                        )
                        .Filter(fi => fi
                            .DateRange(r => r
                                .Field(f => f.timestamp)
                                .GreaterThanOrEquals(leftBound)
                                .LessThan(rightBound)
                            )
                        )
                    )
                )
            );

            lastResponse = searchResponse;
            return searchResponse.Documents;
        }

        //Get the total ammount of hits of the last executed query
        public long getLastResponseHits()
        {
            var test = (ISearchResponse<LogLine>)lastResponse;
            if((test != null) && (test.IsValid))
            {
                return test.HitsMetadata.Total;
            }
            else
            {
                return -1;
            }
            
        }

        public IReadOnlyCollection<LogLine> lastQueryNewPage(int offset, int records)
        {
            switch (lastExecuted)
            {
                case "getAllData":
                    return getAllData(offset, records);
                case "filterOnFilename":
                    return filterOnFilename(offset, records, savedFilename);
                case "filterOnFunction":
                    return filterOnFunction(offset, records, savedFunctionName);
                case "filterOnProcess":
                    return filterOnProcess(offset, records, savedProcessName);
                case "filterOnPID":
                    return filterOnPID(offset, records, savedPID);
                case "filterOnTID":
                    return filterOnTID(offset, records, savedTID);
                case "filterOnLoglevel":
                    return filterOnLoglevel(offset, records, savedLoglevel);
                case "filterOnLogtype":
                    return filterOnLogtype(offset, records, savedLogtype);
                case "filterOnMessage":
                    return filterOnMessage(offset, records, savedMessage);
                default:
                    return null;
            }
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
