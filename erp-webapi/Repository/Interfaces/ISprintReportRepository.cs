using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ISprintReportRepository
    {
        string GetSprintreportLineItemIdForCheckListId(string checkListItemId);

        void AddSprintReportLineItem(string sprintId);

        SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId);

        SprintReportEditModel UpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel);

        List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null,
            int? pageSize = null);
    }
}