using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;



namespace flexli_erp_webapi.DataLayer
{
    public class TagContext:ITagContext
    {

        public TagContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("MongoDbSetting:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("MongoDbSetting:DatabaseName"));
            TaskSearchResult = database.GetCollection<TaskSearch>("task-search-result");
            SprintTasks = database.GetCollection<SprintLabelTask>("sprint-tasks");
            TaskAnchor = database.GetCollection<TaskAnchor>("task-hierarchy");
        }

      
        
        public IMongoCollection<TaskSearch> TaskSearchResult { get; set; }
        
        public IMongoCollection<SprintLabelTask> SprintTasks { get; set; }
        
        public IMongoCollection<TaskAnchor> TaskAnchor { get; set; }
    }
}