using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ISprintReportRepository
    {
      

        void AddOrUpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel);

        SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId);

        List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null,
            int? pageSize = null);
    }
}