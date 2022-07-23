using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.Tag;


namespace flexli_erp_webapi.DataLayer
{
    public class TagContext:ITagContext
    {

        public TagContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("MongoDbSetting:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("MongoDbSetting:DatabaseName"));
            Tag = database.GetCollection<Tag>("tags");
            TagTaskList = database.GetCollection<TagTaskList>("tag-tasklist");
        }

        public  IMongoCollection<Tag> Tag { get; set; }
        
        public IMongoCollection<TagTaskList> TagTaskList { get; set; }
    }
}