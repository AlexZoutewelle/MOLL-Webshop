using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestWebApp.Connector;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.ElasticSearch.Queries
{
    public class EsOLSQuery<T> where T : class
    { 
        private static ConnectionToES _connectionToEs;


        //Readied queries

        public static List<OfferedLabourerService> mostFavourited()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Size(20)
                .Query(q => new BoolQuery { })
                    .Sort(so => so
                        .Field(f => f
                        .Field(ff => ff.fld_addedtowishlist)
                        .Order(SortOrder.Descending)
                        )
                    )
                    .Size(6)
                );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<OfferedLabourerService> packages = response.Documents.ToList<OfferedLabourerService>();
            foreach(OfferedLabourerService package in packages)
            {
                package.fld_cost = package.fld_cost / 100;
            }

            return response.Documents.ToList<OfferedLabourerService>();
        }


        public static List<OfferedLabourerService> Cheapest()
        {
            //Returnt 50 OfferedServices die gesorteerd zijn op most favourited
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Size(20)
                .Query(q => new BoolQuery { })
                    .Sort(so => so
                        .Field(f => f
                        .Field(ff => ff.fld_cost)
                        .Order(SortOrder.Ascending)
                        )
                    )
                    .Size(6)

                );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<OfferedLabourerService> packages = response.Documents.ToList<OfferedLabourerService>();
            foreach (OfferedLabourerService package in packages)
            {
                package.fld_cost = package.fld_cost / 100;
            }

            return response.Documents.ToList<OfferedLabourerService>();
        }

        public static List<OfferedLabourerService> mostBought()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Size(20)
                .Query(q => new BoolQuery { })
                    .Sort(so => so
                        .Field(f => f
                        .Field(ff => ff.fld_timesbought)
                        .Order(SortOrder.Ascending)
                        )
                    )
                    .Size(6)
                );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<OfferedLabourerService> packages = response.Documents.ToList<OfferedLabourerService>();
            foreach (OfferedLabourerService package in packages)
            {
                package.fld_cost = package.fld_cost / 100;
            }

            return response.Documents.ToList<OfferedLabourerService>();
        }




        public static OfferedLabourerService findByOfferedServiceId(int id)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();


            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Size(1)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.fld_offeredserviceid)
                        .Query(id.ToString())
                    )
                )
            );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            OfferedLabourerService package = (OfferedLabourerService)response.Documents.ElementAt(0);
            package.fld_cost = package.fld_cost / 100;

            return package;
        }


        //Generic queries

        //From searchForm

        public static List<OfferedLabourerService> generalSearchQuery(object input, int size)
        {

            //Get the client
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            //Prepare the lists
            List<QueryContainer> mustList = new List<QueryContainer>();

            //Use reflect to get all the properties values and names

            foreach (PropertyInfo property in input.GetType().GetProperties())
            {
                object value = property.GetValue(input);
                if (value != null && value.ToString() != "0" && value.ToString() != "00:00:00" && value.ToString() != "\0")
                {
                    mustList.Add(getFilter(property, input));
                }
            }


            BoolQuery boolQuery1 = new BoolQuery
            {
                Must = mustList, 
            };


            var response1 = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                    .Query(q => boolQuery1)
                    .Size(size));

            var datasend = (from hits in response1.Hits
                            select hits.Source).ToList();

            var json = client.RequestResponseSerializer.SerializeToString(boolQuery1);

            List<OfferedLabourerService> packages = response1.Documents.ToList<OfferedLabourerService>();

            foreach (OfferedLabourerService package in packages)
            {
                package.fld_cost = package.fld_cost / 100;
            }

            return packages;
        }


        public static QueryContainer getFilter(PropertyInfo property, object input)
        {

            object propertyValue = property.GetValue(input);
            //string propertyName = property.Name;
            Type propertyType = propertyValue.GetType();
            //string testString = property.GetValue(input).ToString();
            

            QueryContainer filter = new QueryContainer();

            if (propertyType == typeof(TimeSpan))
            {
                //Search model passed a TimeSpan as one of its query terms
                //In order to query ElasticSearch, we must convert it to a double, representing total seconds since 00:00

                //First, we construct the TimeSpan object
                TimeSpan runTimeObject = (TimeSpan)input.GetType().GetRuntimeProperty(property.Name).GetValue(input);

                //So we can get it's total seconds
                double inSeconds = runTimeObject.TotalSeconds;

                //We need to query the right property, so we should fetch it.
                PropertyInfo propertyToQuery = input.GetType().GetProperty(property.Name + "insec");

                //Finally, we check if we're dealing with timefirst or timelast:
                if (property.Name.EndsWith("first"))
                {

                    filter = new QueryContainerDescriptor<T>()
                        .Range(r => r.Field(propertyToQuery)
                            .GreaterThanOrEquals(inSeconds)
                    );

                }

                else if (property.Name.EndsWith("last"))
                {
                    filter = new QueryContainerDescriptor<T>()
                        .Range(r => r.Field(propertyToQuery)
                          .LessThanOrEquals(inSeconds)
                    );
                }

            }
            else if (propertyType == typeof(DateTime))
            {
                //We hebben geen datecreated voor OLS, maar voor andere wel
                return new QueryContainer();

            }
            else if(propertyType == typeof(int))
            {
                //Lowercost of HigherCost?
                if (property.Name.StartsWith("lower"))
                {
                    filter = new QueryContainerDescriptor<T>()
                        .Range(r => r.Field(input.GetType().GetProperty("fld_cost"))
                          .GreaterThanOrEquals(Convert.ToInt32(propertyValue)*100));
                }
                else if (property.Name.StartsWith("higher"))
                {
                    filter = new QueryContainerDescriptor<T>()
                        .Range(r => r.Field(input.GetType().GetProperty("fld_cost"))
                          .LessThanOrEquals(Convert.ToInt32(propertyValue)*100));
                }
            }
            else
            {
                //String

                filter = new QueryContainerDescriptor<T>()
                .MatchPhrasePrefix(t => t.Field(property).Slop(10).MaxExpansions(10).Query(propertyValue.ToString()));

            }

            return filter;
        }


        //From small searchbar in navigation

        public static List<OfferedLabourerService> simpleSearch(string input)
        {
            //Get the client
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();


            var result = client.Search<OfferedLabourerService>(x => x   // use search method
               .Query(q => q                // define query
                   .MultiMatch(mp => mp         // of type MultiMatch
                       .Query(input)            // pass text
                       .Fields(f => f           // define fields to search against
                        .Fields(f1 => f1.fld_name, f2 => f2.fld_description, f3 => f3.fld_category)))));



            List <OfferedLabourerService> packages = result.Documents.ToList<OfferedLabourerService>();

            foreach (OfferedLabourerService package in packages)
            {
                package.fld_cost = package.fld_cost / 100;
            }

            return packages;

        }

        //Get everything
        public static List<OfferedLabourerService> getAll()
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            var result = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                    .Query(q => q
                        .MatchAll()
                    )
                    .From(0)
                    .Size(10000));

            List<OfferedLabourerService> packages = result.Documents.ToList<OfferedLabourerService>();

            foreach (OfferedLabourerService package in packages)
            {
                package.fld_dateofbirth = package.fld_dateofbirth.Substring(0, 10);
                package.fld_cost = package.fld_cost / 100;
            }

            return packages;

        }

        //Get by ServiceId
        public static List<OfferedLabourerService> getByService(int serviceid)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();


            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.fld_serviceid)
                        .Query(serviceid.ToString())
                    )
                )
            );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<OfferedLabourerService> packages = response.Documents.ToList<OfferedLabourerService>();

            return packages;

        }

        //Get by LabourerId
        public static List<OfferedLabourerService> getByLabourer(int labourerid)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();


            Nest.ISearchResponse<OfferedLabourerService> response = client.Search<OfferedLabourerService>(s => s
                .Index("moll_ols")
                .Type("OLS")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.fld_labourerid)
                        .Query(labourerid.ToString())
                    )
                )
            );

            var datasend = (from hits in response.Hits
                            select hits.Source).ToList();

            List<OfferedLabourerService> packages = response.Documents.ToList<OfferedLabourerService>();

            return packages;

        }

    }


}
