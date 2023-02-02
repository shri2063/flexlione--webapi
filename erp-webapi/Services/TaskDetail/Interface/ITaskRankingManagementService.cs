using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ITaskRankingManagementService
    {
        Task<List<TaskDetailEditModel>> GetChildTaskRankingForTask(String parentTaskId);

        Task<List<String>> UpdateRankingOfTask(TaskDetailEditModel task);

        Task<List<String>> RemoveRankingOfTask(TaskDetailEditModel task);





    }
}