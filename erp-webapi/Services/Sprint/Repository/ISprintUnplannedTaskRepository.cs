using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ISprintUnplannedTaskRepository
    {
        SprintUnplannedTaskDataEditModel GetUnplannedTaskScoreData(string sprintId, string taskId);

        SprintUnplannedTaskDataEditModel CreateOrUpdateSprintUnplannedTask(
            SprintUnplannedTaskDataEditModel unplannedTaskEdit);
    }
}