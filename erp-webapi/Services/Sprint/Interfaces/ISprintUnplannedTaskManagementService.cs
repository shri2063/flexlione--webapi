using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ISprintUnplannedTaskManagementService
    {

        SprintUnplannedTaskScoreEditModel RequestHours(string sprintId, string taskId, int hours, string profileId);


        SprintUnplannedTaskScoreEditModel ApproveHours(string sprintId, string taskId, int hours, string profileId);

        List<TaskDetailEditModel> GetUnPlannedTasksForSprint(string sprintId);

    }
}