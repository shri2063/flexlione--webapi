using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class SprintReportManagementService
    {

        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ICheckListRelationRepository _checkListRelationRepository;
        public SprintReportManagementService(ISprintRepository sprintRepository, ICheckListRepository checkListRepository, 
            ISprintReportRepository sprintReportRepository, ITaskRelationRepository taskRelationRepository, 
            ITaskRepository taskRepository, ICheckListRelationRepository checkListRelationRepository)
        {
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
            _sprintReportRepository = sprintReportRepository;
            _taskRelationRepository = taskRelationRepository;
            _taskRepository = taskRepository;
            _checkListRelationRepository = checkListRelationRepository;
        }
        
        

       
        public  SprintReportEditModel ReviewCheckList(SprintReportEditModel sprintReportEditModel, string approverId)
        {
            // If checklist exist - get Sprint Status from DB
            var existingSprintReportLineItem = _sprintReportRepository.GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
            if (existingSprintReportLineItem == null)
            {
                throw new KeyNotFoundException("Sprint report does not exist" + sprintReportEditModel.SprintReportLineItemId);
            }

            var sprint = _sprintRepository.GetSprintById(existingSprintReportLineItem.SprintId);
            
            // [check] Approver id is valid

            if (!ProfileManagementService.CheckManagerValidity(sprint.Owner, approverId))
            {
                throw new ConstraintException("Not valid approver Id: " + approverId);
            }
                
                
            //[Check]: Sprint is not reviewed
            if (sprint.Status != SStatus.Reviewed)
            {
                using (var db = new ErpContext())
                {

                    SprintReportLineItem sprintReportLineItem = db.SprintReport
                        .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportEditModel.SprintReportLineItemId);
                    
                    if (sprintReportEditModel.ManagerComment != null)
                    {
                        sprintReportLineItem.ManagerComment = sprintReportEditModel.ManagerComment;
                    }
                    
                    if (sprintReportEditModel.Approved != null)
                    {
                        sprintReportLineItem.Approved = sprintReportEditModel.Approved.ToString();
                    }
                   
                 
                    db.SaveChanges();
                }
            }

            return _sprintReportRepository.GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
        }
       
        
        
        

      

       

        public static void UpdateProvisionalScoreInSprintReport(string sprintId)
        {
            List<SprintReportLineItem> sprintReports;
            using (var db = new ErpContext())
            {
                sprintReports = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .ToList();
                
                sprintReports.ForEach(sprintReport =>
                {
                    if (sprintReport.Status == CStatus.Completed.ToString())
                    {
                        if(sprintReport.ResultType==CResultType.Numeric.ToString() && Convert.ToInt32(sprintReport.Result)>=sprintReport.WorstCase && Convert.ToInt32(sprintReport.Result) <= sprintReport.BestCase)
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }

                        if (sprintReport.ResultType == CResultType.Boolean.ToString() && sprintReport.Result == "true")
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }

                        if (sprintReport.ResultType == CResultType.File.ToString() && sprintReport.Result != null)
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }
                    }
                });
            }
        }

        public List<SprintReportEditModel> AddSprintReportLineItemsForSprint(String sprintId)
        {
           
                List<String> taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);

                foreach (var taskId in taskIds)
                {

                    List<CheckListItemEditModel> checkListItems =
                        _checkListRepository.GetCheckList(taskId, EAssignmentType.Task);

                    // It is necessary to have atleast one checklist for each task
                    // in order to make sprint - task - checklist pair entry
                    // So if task with no checklist added to sprint,
                    // create a new dummy checklist for that task

                    if (checkListItems.Count == 0)
                    {
                        CheckListItemEditModel dummyNewChecklistItem = _checkListRelationRepository.AddNewChecklistItemForTaskWithNoChecklist(taskId);
                        checkListItems.Add(dummyNewChecklistItem);
                    }

                    foreach (var checkListItem in checkListItems)
                    {
                        SprintReportEditModel sprintReport = new SprintReportEditModel()
                        {
                            SprintReportLineItemId = "server-generated",
                            SprintId = sprintId,
                            TaskId = taskId,
                            TaskDescription =   _taskRepository.GetTaskById(taskId).Description,
                            CheckListItemId = checkListItem.CheckListItemId,
                            Description = checkListItem.Description,
                            ResultType = checkListItem.ResultType,
                            UserComment = checkListItem.UserComment,
                            Approved = SApproved.NoAction,
                            Status = CStatus.NotCompleted,
                            WorstCase = checkListItem.WorstCase,
                            BestCase = checkListItem.BestCase,
                            Score = 0
                        };
                        _sprintReportRepository.AddOrUpdateSprintReportLineItem(sprintReport);
                    }
                }

                return _sprintReportRepository.GetSprintReportForSprint(sprintId);
        }
        
       
    }
}