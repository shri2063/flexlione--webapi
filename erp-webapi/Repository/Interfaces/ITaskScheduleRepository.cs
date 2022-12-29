using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskScheduleRepository
    {
        TaskScheduleEditModel GetTaskScheduleByIdFromDb(string taskScheduleId);
    }
}