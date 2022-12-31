using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace mflexli_erp_webapi.Repository.Interfaces
{
    public interface ICheckListRepository
    {
        List<CheckListItemEditModel> GetCheckList(string taskId, EAssignmentType type, int? pageIndex = null,
            int? pageSize = null);

        CheckListItemEditModel CreateOrUpdateCheckListInDb(CheckListItemEditModel checkListItemEditModel);

        CheckListItemEditModel GetCheckListById(string checkListItemId);
    }
}