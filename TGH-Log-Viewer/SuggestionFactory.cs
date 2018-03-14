using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TGH_Log_Viewer
{
    class SuggestionFactory
    {
        public List<String> getSuggestionsFromJson(String json)
        {
            List<String> suggestionCollection = new List<String>();

            var jsonObj = JObject.Parse(json);

            if (jsonObj["error"] == null)
            {
                foreach (var bucket in jsonObj["aggregations"]["logtypes"]["buckets"])
                {
                    String key = (String)bucket["key"];
                    suggestionCollection.Add(key);
                }
            }
            else Console.WriteLine("ERROR - Returned json contains an error message!");
      
            return suggestionCollection;
        }
    }
}
