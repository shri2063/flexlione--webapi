using flexli_erp_webapi.BsonModels;
using MongoDB.Driver;

namespace flexli_erp_webapi.DataLayer.Interface
{
    public interface ITagContext
    {
        
        
         IMongoCollection<TaskSearch> TaskSearchResult { get; }
         
         IMongoCollection<SprintLabelTask> SprintTasks { get; }
         
         IMongoCollection<TaskAnchor> TaskAnchor { get; set; }
    }
}