using System;
using System.Threading.Tasks;
using m_sort_server.BsonModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S = Swashbuckle.AspNetCore.Swagger.Tag;

namespace m_sort_server.Services
{
    public class TagManagementService
    {
        public static SearchTag AddTag(string search)
        {
            
            MongoClient dbClient = new MongoClient("mongodb+srv://shrikant:Flexli123@freecluster.6nw7e.mongodb.net");

            var database = dbClient.GetDatabase("tags");
            var collection = database.GetCollection<BsonDocument>("search_tags");

            SearchTag searchTag = new SearchTag();
            searchTag.Id = "9";
            searchTag.Description = "Conveyor";
            searchTag.Tasks = new []{ new TaskSearchView(){ TaskId = "3", Owner = "Viru",Description = "Conveyor mfg"} };

            var documentNext = searchTag.ToBsonDocument();
            collection.InsertOne(documentNext);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "9");
            var studentDocument = collection.Find(filter).FirstOrDefault().ToBsonDocument();
            SearchTag myObj = BsonSerializer.Deserialize<SearchTag>(studentDocument);
            return myObj;

        }
    }
}