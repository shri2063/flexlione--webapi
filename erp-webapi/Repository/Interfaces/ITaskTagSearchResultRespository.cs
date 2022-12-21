using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITagTaskListRepository
    {
        Task<TagTaskList> GetTagTaskListForTag(string keyword, ETagType tagType);
        
        Task<TagTaskList> CreateTagTaskListForTag( string keyword, ETagType tagType);
        
        Task<TagTaskList> AddTaskToTagTask(string keyword, List<string> taskIdList, ETagType tagType);

        Task<TagTaskList> ReviseTaskListForSearchTag(string keyword);
        
        Task<TagTaskList> RemoveSearchTaskListFromTag(string keyword, ETagType tagType,  string taskId = null);
        
        Task<bool> DeleteTagTaskList(string keyword, ETagType tagType);
        
        Task<bool> ParseTaskDescriptionForSearchTags(string description, string taskId, IEnumerable<TaskTag> tags);
    }
}