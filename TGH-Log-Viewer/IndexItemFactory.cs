using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    class IndexItemFactory
    {
        public List<IndexItem> getIndexItemsFromJson(String json)
        {
            List<IndexItem> items = new List<IndexItem>();

            if (json != "")
            {
                var jsonObj = JObject.Parse("{\"indices\":"+json+"}");
                if (jsonObj["error"] == null)
                {
                    foreach (var indxitem in jsonObj["indices"])
                    {
                        
                        IndexItem indItem = new IndexItem((String)indxitem["index"], (String)indxitem["docs.count"], (String)indxitem["store.size"]);
                        items.Add(indItem);
                    }

                    return items;
                }
                else
                {
                    Console.WriteLine("ERROR - Returned json contains an error message!");
                }
            }
            else
            {
                Console.WriteLine("ERROR - Returned json is empty!");
            }

            return items;

        }
    }
}
