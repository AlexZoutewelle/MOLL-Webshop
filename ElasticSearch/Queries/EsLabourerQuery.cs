using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Connector;
using TestWebApp.Models;
using Elasticsearch.Net;
using Nest;

namespace TestWebApp.ElasticSearch.Queries
{
    public static class EsLabourerQuery
    {
        private static ConnectionToES _connectionToEs;


        public static tbl_labourerdata FindById(int labourerId)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            var response = client.Search<tbl_labourerdata>(s => s
                .Index("moll_labourers")
                .Type("Labourer")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.fld_labourerid)
                        .Query(labourerId.ToString())
                    )
                )
            );

            
            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();


            List<tbl_labourerdata> result = response.Documents.ToList();
            return result[0];

        }

        public static List<tbl_labourerdata> AllLabourers()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();



            BoolQuery boolQuery1 = new BoolQuery
            {

            };

            Nest.ISearchResponse<tbl_labourerdata> response = client.Search<tbl_labourerdata>(s => s
                .Index("moll_labourers")
                .Type("Labourer")
                    .Query(q => q
                        .MatchAll()
                    )
                    .From(0)
                    .Size(1000)
                );

            var json = client.RequestResponseSerializer.SerializeToString(boolQuery1);


            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<tbl_labourerdata> packages = response.Documents.ToList<tbl_labourerdata>();

            foreach(tbl_labourerdata labourer in packages)
            {
                labourer.fld_dateofbirth = labourer.fld_dateofbirth.Substring(0, 10);
            }


            return response.Documents.ToList<tbl_labourerdata>();
        }
    }
}
