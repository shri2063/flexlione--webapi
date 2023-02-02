using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskRelationRepository
    {
        List<string> GetTaskIdsForSprint(string sprintId);
        TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId);

        TaskDetailEditModel RemoveTaskFromSprint(string taskId);


    }
}