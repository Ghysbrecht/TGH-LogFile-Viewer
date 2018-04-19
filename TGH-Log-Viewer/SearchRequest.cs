using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    public class SearchRequest
    {
        public String searchTerm { get; set; }
        public String searchColumn { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int offset { get; set; }
        public int records { get; set; }

        public SearchRequest(String searchTerm, String searchColumn, DateTime startDate , DateTime endDate, int offset, int records)
        {
            this.searchTerm = searchTerm;
            this.searchColumn = searchColumn;
            this.startDate = startDate;
            this.endDate = endDate;
            this.offset = offset;
            this.records = records;
        }

        public SearchRequest cloneThis()
        {
            return new SearchRequest(searchTerm, searchColumn, startDate, endDate, offset, records);
        }
    }
}
