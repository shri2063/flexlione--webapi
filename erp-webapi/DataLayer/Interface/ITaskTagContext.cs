using flexli_erp_webapi.BsonModels;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.TaskTag;

namespace flexli_erp_webapi.DataLayer.Interface
{
    public interface ITagContext
    {
         IMongoCollection<TaskTag> TaskTag { get; }
        
         IMongoCollection<TagTaskList> TaskTagSearchResult { get; }
         
         IMongoCollection<SprintLabelTask> SprintTasks { get; }
    }
}