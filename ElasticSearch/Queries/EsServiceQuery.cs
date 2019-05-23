using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Connector;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using TestWebApp.Models;

namespace TestWebApp.ElasticSearch.Queries
{
    public static class EsServiceQuery
    {
        private static ConnectionToES _connectionToEs;


        public static tbl_servicedata FindById(int serviceId)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            var response = client.Search<tbl_servicedata>(s => s
                .Index("moll_dataservices")
                .Type("Services")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.fld_serviceid)
                        .Query(serviceId.ToString())
                    )
                )
            );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<tbl_servicedata> result = response.Documents.ToList<tbl_servicedata>();

            return result[0];

            
        }


        public static List<tbl_servicedata> AllServices()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();



            BoolQuery boolQuery1 = new BoolQuery
            {

            };

            Nest.ISearchResponse<tbl_servicedata> response = client.Search<tbl_servicedata>(s => s
                .Index("moll_dataservices")
                .Type("Services")
                    .Query(q => q
                        .MatchAll()
                    )
                    .From(0)
                    .Size(1000)
                );

            var json = client.RequestResponseSerializer.SerializeToString(boolQuery1);


            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<tbl_servicedata> packages = response.Documents.ToList<tbl_servicedata>();


            return response.Documents.ToList<tbl_servicedata>();
        }
    }
}
