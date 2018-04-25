using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TGH_Log_Viewer
{
    public class AppSettings
    {
        // DEFAULT VALUES --------------------------------------------------
        private const string defElasticip = "localhost:9200";
        private const string defDefaultIndex = "tghlogstasher";
        private const int defDefaultRecords = 100;
        private const bool defAutoTime = false;
        private const bool defAutoScroll = true;
        private const string defFilename = "DefaultAppSettings.xml";
        private const int defThemeNumber = 1;
        // -----------------------------------------------------------------

        public string elasticip { get; set; }
        public string defaultIndex { get; set; }
        public int defaultRecords { get; set; }
        public int themeNumber { get; set; }
        public bool autoTime { get; set; }
        public bool autoScroll { get; set; }
        public List<FileExclusion> exclusions { get; set; }

        private string saveLocation;

        public AppSettings(String elasticip, string defaultIndex)
        {
            String executingPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            saveLocation = executingPath + "\\" + defFilename;
            this.elasticip = elasticip;
            this.defaultIndex = defaultIndex;
            defaultRecords = defDefaultRecords;
            autoTime = defAutoTime;
            autoScroll = defAutoScroll;
            themeNumber = defThemeNumber;
            exclusions = new List<FileExclusion>();
        }

        public AppSettings() : this(defElasticip, defDefaultIndex)
        {
       
        }

        public void saveToFile()
        {
            Console.WriteLine("Saving to file -> " + saveLocation);
            using (StreamWriter sw = new StreamWriter(saveLocation))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                xmls.Serialize(sw, this);
            }
        }

        public AppSettings restoreFromFile()
        {
            if (File.Exists(saveLocation))
            {
                using (StreamReader sw = new StreamReader(saveLocation))
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                    return xmls.Deserialize(sw) as AppSettings;
                }
            }
            else return this;
        }
    }
}
