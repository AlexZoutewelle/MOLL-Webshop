using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Connector
{
    //De class gebruikt om connectie te maken met ElasticSearch
    public class ConnectionToES
    {
        public ElasticClient EsClient()
        {
            var nodes = new Uri[]
            {
                new Uri("http://localhost:9200")
                //ElasticSearch nodes zitten op localhost:9200
                //TODO: Remote ElasticSearch server?
            };

            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DefaultIndex("packages");
            var elasticClient = new ElasticClient(connectionSettings);

            return elasticClient;
        }
    }
}
