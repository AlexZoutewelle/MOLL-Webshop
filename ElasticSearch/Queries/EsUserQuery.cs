using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Connector;
using TestWebApp.Models;

namespace TestWebApp.ElasticSearch.Queries
{
    public class EsUserQuery
    {
        private static ConnectionToES _connectionToEs;

        public static List<tbl_userdata> AllUsers()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();



            BoolQuery boolQuery1 = new BoolQuery
            {
               
            };

            Nest.ISearchResponse<tbl_userdata> response = client.Search<tbl_userdata>(s => s
                .Index("moll_users")
                .Type("User")
                    .Query(q => q
                        .MatchAll()
                        
                    )
                    .From(0)
                    .Size(1000)
                );

            var json = client.RequestResponseSerializer.SerializeToString(boolQuery1);


            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<tbl_userdata> packages = response.Documents.ToList<tbl_userdata>();


            return response.Documents.ToList<tbl_userdata>();
        }
    }
}
