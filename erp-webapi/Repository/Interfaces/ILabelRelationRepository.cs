using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ILabelRelationRepository
    {
        Task<List<TaskSearchView>> GetSprintLabelTaskForProfileId(string profileId);

        Task<List<TaskSearchView>> GetNotCompleteLabelTaskForProfileId(string profileId);

        Task<SprintLabelTask> AddSprintLabelToTask(string taskId);

    }
}