using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ISprintUnplannedTaskRepository
    {
        SprintUnplannedTaskScoreEditModel GetUnplannedTaskScoreData(string sprintId, string taskId);

        SprintUnplannedTaskScoreEditModel CreateOrUpdateSprintUnplannedTask(
            SprintUnplannedTaskScoreEditModel unplannedTaskEdit);
    }
}