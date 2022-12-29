using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ISprintRelationRepository
    {
        List<SprintEditModel> GetSprintsForProfileId(string profileId, int? pageIndex = null, int? pageSize = null);
    }
}