using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Services.TaskSearch
{
    public interface ITaskSearchResultRepository
    {
        Task<List<TaskSearchView>> SearchTasks(string keyword);
        
        Task<List<TaskSearchView>> AddOrUpdateTask(string keyword, TaskSearchView task);
        

        Task<List<TaskSearchView>> RemoveTask(string keyword, string taskId);

        Task<List<string>> GetListOfTaskSearch();

        


    }
}