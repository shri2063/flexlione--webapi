using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ILabelRepository
    {
        Task<List<TaskSearchView>> SprintLabelTaskForProfileId(string profileId);

        Task<List<TaskSearchView>> notCompleteLabelTaskForProfileId(string profileId);

        Task<SprintLabelTask> AddSprintLabelToTask(string taskId);

    }
}