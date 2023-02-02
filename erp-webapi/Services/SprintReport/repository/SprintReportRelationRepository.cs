using System.Linq;
using flexli_erp_webapi.Repository.Interfaces;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class SprintReportRelationRepository:ISprintReportRelationRepository
    {
        
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        public SprintReportRelationRepository(ISprintRepository sprintRepository, ICheckListRepository checkListRepository)
        {
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
        }
        public string GetSprintReportLineItemIdForCheckListId(string checkListItemId)
        {
             using (var db = new ErpContext())
                {
                
                    var sprintReport = db.SprintReport
                        .Where(x => x.CheckListItemId == checkListItemId)
                        .ToList();
                    // [Check] Multiple sprint report line item can have same checklist Item id. Use one with  sprint status not closed
                    var reqSprintReport = sprintReport.Find(x => ! _sprintRepository.GetSprintById(x.SprintId)
                        .Closed);


                    return reqSprintReport is null? null: reqSprintReport.SprintReportLineItemId ;
                }
            
        }
    }
}