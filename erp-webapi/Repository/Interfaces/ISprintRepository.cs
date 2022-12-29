using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ISprintRepository
    {
        SprintEditModel GetSprintById(string sprintId);
    }
}