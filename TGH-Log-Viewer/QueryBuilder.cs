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
        //Get all the data in the main index where field = X and logtype = Y
        public IReadOnlyCollection<LogLine> getSpecificData(ElasticClient client, int offset, int records)
        {
            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchPhrase(c => c
                                .Field(p => p.filename)
                                .Query("SLWatchDog")
                            ), m => m
                            .MatchPhrase(c => c
                                .Field(p => p.logtype)
                                .Query("DBG")
                            )
                        )
                    )
                )
            );

            return searchResponse.Documents;
        }

        //Get all the data in the main index
        public IReadOnlyCollection<LogLine> getAllData(ElasticClient client, int offset, int records)
        {
            var searchResponse = client.Search<LogLine>(s => s
                .AllTypes()
                .From(offset)
                .Size(records)
                .Query(q => q
                    .MatchAll()
                )
            );

            return searchResponse.Documents;
        }
    }
}
