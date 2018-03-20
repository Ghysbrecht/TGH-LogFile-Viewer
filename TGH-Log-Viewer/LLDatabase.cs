using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace TGH_Log_Viewer
{
    class LLDatabase
    {
        ElasticLowLevelClient client;
        bool valid;
        //String defaultIndex;
        public LLDatabase(string address, string index)
        {
            Console.WriteLine("Creating client!");
            var settings = new ConnectionConfiguration(new Uri("http://" + address))
                    .DisableDirectStreaming()
                    .ThrowExceptions()
                    .OnRequestCompleted(request =>
                    {
                        var builder = new StringBuilder();
                        builder.AppendFormat("URL: {0}", request.Uri.ToString());
                        if (request.RequestBodyInBytes != null) builder.AppendFormat("\r\nRequest:\r\n\t{0}", Encoding.UTF8.GetString(request.RequestBodyInBytes).Replace("\n", "\n\t"));
                        if (request.ResponseBodyInBytes != null) builder.AppendFormat("\r\nResponse:\r\n\t{0}", Encoding.UTF8.GetString(request.ResponseBodyInBytes).Replace("\n", "\n\t"));
                        Console.WriteLine(builder.ToString());
                    });

            try {
                client = new ElasticLowLevelClient(settings);
                client.Ping<StringResponse>();
                valid = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while trying to create a client! -> " + e.Message);
                valid = false;
            }
            
        }

        public ElasticLowLevelClient getClient()
        {
            return client;
        }

        //Check if the connection is valid
        public bool isValid()
        {
            return valid;
        }
    }
}
