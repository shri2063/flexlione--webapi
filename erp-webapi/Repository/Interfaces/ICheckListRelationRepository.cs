using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ICheckListRelationRepository
    {
        CheckListItemEditModel AddNewChecklistItemForTaskWithNoChecklist(string taskId);
    }
}