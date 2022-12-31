using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Services.TaskSearch
{
    public interface ITaskTagSearchResultRepository
    {
        Task<List<TaskSearchView>> GetTaskListForTag(string keyword);
        
        Task<List<TaskSearchView>> AddTaskToTaskListOfTag(string keyword, string taskId);

        Task<TaskTag> CreateTag(string keyword);

        Task<List<TaskSearchView>> RemoveTaskFromTaskListOfTag(string keyword, string taskId);

        Task<List<string>> GetListOfTaskTags();
    }
}