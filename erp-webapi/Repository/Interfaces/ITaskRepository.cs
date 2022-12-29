using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskRepository
    {
        TaskDetailEditModel GetTaskById(string taskId);

        TaskDetailEditModel CreateOrUpdateTask(TaskDetailEditModel task);

        bool DeleteTask(string taskId);

        List<string> GetChildTaskIdList(string parentTaskId);

        List<TaskShortDetailEditModel> GetTaskIdList(string parentTaskId = null, int? pageIndex = null,
            int? pageSize = null);

    }
}