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


        public SearchRequest(String searchTerm, String searchColumn)
        {
            this.searchTerm = searchTerm;
            this.searchColumn = searchColumn;
        }
    }
}
