using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    public class AppSettings
    {
        // DEFAULT VALUES --------------------------------------------------
        private const string defElasticip = "localhost:9200";
        private const string defDefaultIndex = "maintest";
        private const int defDefaultRecords = 100;
        private const bool defAutoTime = false;
        // -----------------------------------------------------------------

        public string elasticip { get; set; }
        public string defaultIndex { get; set; }
        public int defaultRecords { get; set; }
        public bool autoTime { get; set; }

        public AppSettings(String elasticip, string defaultIndex)
        {
            this.elasticip = elasticip;
            this.defaultIndex = defaultIndex;
            defaultRecords = defDefaultRecords;
            autoTime = defAutoTime;
        }

        public AppSettings() : this(defElasticip, defDefaultIndex)
        {
       
        }
    }
}
