using flexli_erp_webapi.BsonModels;
using MongoDB.Driver;

namespace flexli_erp_webapi.DataLayer.Interface
{
    public interface ITagContext
    {
        
        
         IMongoCollection<TaskTag> TaskTagSearchResult { get; }
         
         IMongoCollection<SprintLabelTask> SprintTasks { get; }
         
         IMongoCollection<TaskHierarchy> TaskHierarchy { get; set; }
    }
}