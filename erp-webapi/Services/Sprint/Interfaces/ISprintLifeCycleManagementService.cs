using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ISprintLifeCycleManagementService
    {
        SprintEditModel RequestForApproval(string sprintId, string userId);

        SprintEditModel ApproveSprint(string sprintId, string approverId);

        SprintEditModel RequestForClosure(string sprintId, string userId);

        SprintEditModel CloseSprint(string sprintId, string approverId);

        SprintEditModel ReviewCompleted(string sprintId, string approverId);
    }
}