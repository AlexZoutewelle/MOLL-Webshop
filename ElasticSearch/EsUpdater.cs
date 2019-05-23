using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Connector;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.ElasticSearch
{
    public static class EsUpdater<T> where T : class
    {
        private static ConnectionToES _connectionToEs;

        public static void UpdateField(string documentPath, string fieldName, object fieldValue)
        {
            try
            {
                _connectionToEs = new ConnectionToES();
                var client = _connectionToEs.EsClient();

                var scriptParams = new Dictionary<string, object>
            {
                {fieldName, fieldValue }
            };

                string scriptString = $"ctx._source." + fieldName + " = params." + fieldName;


                var response = client.Update<T>(
                    documentPath //This is your document path
                    , request => request.Script(
                        script =>
                            script.Inline(
                                            scriptString 

                                )
                                .Params(scriptParams)));


                var json = client.RequestResponseSerializer.SerializeToString(response);

            }
            catch (Exception e)
            {

            }
        }

        public static void UpsertDocument(T document, string index, string type, int documentId)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            try
            {
                var responseUpdate = client.Update<T, Object>(document, u => u
                 .Index(index)
                 .Type(type)
                 .Doc(document)
                 .DocAsUpsert(true));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void InsertDocument(T document, string index, string type, string id)
        {
            try
            {
                _connectionToEs = new ConnectionToES();
                var client = _connectionToEs.EsClient();

                client.Index<T>(document, i => i.Index(index).Id(id).Refresh(Refresh.True));
            }
            catch(Exception e)
            {

            }
        }

        public static void DeleteDocument(int id, string type, string index)
        {
            _connectionToEs = new ConnectionToES();
            var client = _connectionToEs.EsClient();

            var responseDelete = client.Delete<T>(id, d => d
                                                        .Type(type)
                                                        .Index(index));
        }

    }
}
