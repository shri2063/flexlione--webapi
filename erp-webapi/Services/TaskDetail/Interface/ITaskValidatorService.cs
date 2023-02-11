using System;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ITaskValidatorService
    {
        Boolean CheckValidAssigneeFields(TaskDetailEditModel taskForUpdate, TaskDetailEditModel existingTask,
            string loggedInId);

        Boolean CheckIfDeadlineUpdatedByManager(TaskDetailEditModel taskForUpdate, string loggedInId);

        Boolean CheckUpdatedFields(TaskDetailEditModel taskForUpdate);

        Boolean GetPickedUpStatus(TaskDetailEditModel taskForUpdate);
        
        
    }
}