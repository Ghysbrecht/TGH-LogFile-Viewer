using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace TGH_Log_Viewer
{
    public class Database
    {
        ElasticClient client;
        public Database(string address, string index)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                    .DefaultIndex("maintest")
                    .DisableDirectStreaming()
                    .DefaultMappingFor<LogLine>(m => m
                        .PropertyName(f => f.timestamp, "@timestamp")
                        .PropertyName(f => f.TID, "TID")
                        .PropertyName(f => f.PID, "PID"))
                    .OnRequestCompleted(request =>
                    {
                        var builder = new StringBuilder();
                        builder.AppendFormat("URL: {0}", request.Uri.ToString());
                        if (request.RequestBodyInBytes != null) builder.AppendFormat("\r\nRequest:\r\n\t{0}", Encoding.UTF8.GetString(request.RequestBodyInBytes).Replace("\n", "\n\t"));
                        if (request.ResponseBodyInBytes != null) builder.AppendFormat("\r\nResponse:\r\n\t{0}", Encoding.UTF8.GetString(request.ResponseBodyInBytes).Replace("\n", "\n\t"));
                        Console.WriteLine(builder.ToString());
                    });
            client = new ElasticClient(settings);
        }

        public ElasticClient getClient()
        {
            return client;
        }

        //Check if the connection is valid
        public bool isValid()
        {
            return client.NodesStats().IsValid;
        }
    }
}
