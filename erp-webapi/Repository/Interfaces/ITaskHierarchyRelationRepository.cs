using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace m_sort_server.Repository.Interfaces
{
    public interface ITaskHierarchyRelationRepository
    {
        List<TaskHierarchyEditModel> UpdateTaskHierarchy(string taskId = null);

        TaskHierarchyEditModel GetTaskHierarchyByTaskIdFromDb(string taskId);

        void DeleteTaskHierarchy(string taskId);

        List<string> GetDownStreamTaskIdsForTaskId(string taskId);


    }
}