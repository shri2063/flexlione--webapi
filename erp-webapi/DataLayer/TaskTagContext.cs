using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.TaskTag;


namespace flexli_erp_webapi.DataLayer
{
    public class TagContext:ITagContext
    {

        public TagContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("MongoDbSetting:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("MongoDbSetting:DatabaseName"));
            TaskTag = database.GetCollection<TaskTag>("task-tags");
            TaskTagSearchResult = database.GetCollection<TagTaskList>("task-tag-search-result");
            SprintTasks = database.GetCollection<SprintLabelTask>("sprint-tasks");
        }

        public  IMongoCollection<TaskTag> TaskTag { get; set; }
        
        public IMongoCollection<TagTaskList> TaskTagSearchResult { get; set; }
        
        public IMongoCollection<SprintLabelTask> SprintTasks { get; set; }
    }
}