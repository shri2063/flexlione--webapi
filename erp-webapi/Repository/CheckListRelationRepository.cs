using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class CheckListRelationRepository: ICheckListRelationRepository
    {
       
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        public CheckListRelationRepository(ISprintRepository sprintRepository, ICheckListRepository checkListRepository)
        {
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
        }
        public CheckListItemEditModel AddNewChecklistItemForTaskWithNoChecklist(string taskId)
        {
            var checkListId = "newChecklist";
            using (var db = new ErpContext())
            {
                TaskDetail task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);

                // new checklist item with description same as task, result type boolean and essential = true
                CheckListItemEditModel dummyNewChecklistItem = new CheckListItemEditModel()
                {
                    CheckListItemId = checkListId,
                    Description = task.Description,
                    TypeId = task.TaskId,
                    Status = CStatus.NotCompleted,
                    WorstCase = 0,
                    BestCase = 0,
                    ResultType = CResultType.Boolean,
                    Essential = true,
                    AssignmentType = EAssignmentType.Task
                };

                return _checkListRepository.CreateOrUpdateCheckListInDb(dummyNewChecklistItem);
            }
        }
    }
}