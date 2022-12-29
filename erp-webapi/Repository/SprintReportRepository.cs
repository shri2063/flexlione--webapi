using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class SprintReportRepository: ISprintReportRepository

    {
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        public SprintReportRepository(ISprintRepository sprintRepository, ICheckListRepository checkListRepository)
        {
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
        }
        
        public  List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
        {
          
            
            List<SprintReportEditModel> sprintReport = new List<SprintReportEditModel>();
            List<string> sprintReportLineItemIds = new List<string>();
            
            sprintReportLineItemIds = GetSprintReportLineItemIdsForSprint(sprintId, pageIndex, pageSize);

            if (sprintReportLineItemIds.Count == 0)
            {
                return sprintReport;
            }
            
            sprintReportLineItemIds.ForEach(x =>
            {
                sprintReport.Add(GetSprintReportItemById(x));
            });
            return sprintReport;
        }
        
        private static List<string> GetSprintReportLineItemIdsForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
        {
            List<string> sprintReportLineItemIds = new List<string>();
            using (var db = new ErpContext())
            {
                // [Check] : Pagination
                if (pageIndex != null && pageSize != null)
                {
                    if (pageIndex <= 0 || pageSize <= 0)
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                    // skip take logic
                    sprintReportLineItemIds = db.SprintReport
                        .Where(x => x.SprintId == sprintId)
                        .Select(y => y.SprintReportLineItemId).AsEnumerable()
                        .OrderByDescending(Convert.ToInt32)
                        .Skip(((int) pageIndex - 1) * (int) pageSize)
                        .Take((int) pageSize)
                        .ToList();

                    if (sprintReportLineItemIds.Count == 0)
                    {
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                    }

                    return sprintReportLineItemIds;
                }
                
                sprintReportLineItemIds = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(y => y.SprintReportLineItemId).AsEnumerable()
                    .OrderByDescending(Convert.ToInt32)
                    .ToList();


                return sprintReportLineItemIds;
            }
        }

        
        
        public  string GetSprintreportLineItemIdForCheckListId(string checkListItemId)
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

        public void AddSprintReportLineItem(string sprintId)
        {
            using (var db = new ErpContext())
            {
                SprintEditModel sprint = _sprintRepository.GetSprintById(sprintId);

                if (sprint == null)
                {
                    throw new KeyNotFoundException("invalid sprint id, sprint doesn't exist");
                }

                List<string> tasks = db.TaskDetail
                    .Where(task => task.SprintId == sprintId)
                    .Select(task => task.TaskId)
                    .ToList();

                foreach (var task in tasks)
                {
                    // TaskDetailEditModel taskDetailEditModel = TaskManagementService.GetTaskById(task);

                    // Fetching description of given task
                    string taskDescription = db.TaskDetail
                        .Where(x => x.TaskId == task)
                        .Select(x => x.Description)
                        .SingleOrDefault();

                    List<CheckListItemEditModel> checkListItems =
                        _checkListRepository.GetCheckList(task, ECheckListType.Task);

                    // It is necessary to have atleast one checklist for each task
                    // in order to make sprint - task - checklist pair entry
                    // So if task with no checklist added to sprint,
                    // create a new dummy checklist for that task

                    if (checkListItems.Count == 0)
                    {
                        CheckListItemEditModel dummyNewChecklistItem = AddNewChecklistItemForTaskWithNoChecklist(task);
                        checkListItems.Add(dummyNewChecklistItem);
                    }

                    foreach (var checkListItem in checkListItems)
                    {
                        SprintReport sprintReport = new SprintReport()
                        {
                            SprintReportLineItemId = GetNextAvailableId(),
                            SprintId = sprintId,
                            TaskId = task,
                            TaskDescription = taskDescription,
                            CheckListItemId = checkListItem.CheckListItemId,
                            Description = checkListItem.Description,
                            ResultType = checkListItem.ResultType.ToString(),
                            UserComment = checkListItem.UserComment,
                            Approved = SApproved.NoAction.ToString(),
                            Status = CStatus.NotCompleted.ToString(),
                            WorstCase = checkListItem.WorstCase,
                            BestCase = checkListItem.BestCase,
                            Score = 0
                        };

                        db.SprintReport.Add(sprintReport);
                        db.SaveChanges();
                    }
                }
            }
        }
        
        public  SprintReportEditModel UpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel)
        {
            SprintReport sprintReport;
            
            
            using (var db = new ErpContext())
            {

                sprintReport = db.SprintReport
                                   .FirstOrDefault(x => x.SprintId == sprintReportEditModel.SprintId && x.CheckListItemId == sprintReportEditModel.CheckListItemId)
                               ?? throw new KeyNotFoundException("Sprint report Id does not exist: " +
                                                                 sprintReportEditModel.SprintId);
                switch (_sprintRepository.GetSprintById(sprintReportEditModel.SprintId).Closed)
                {
                    case false:
                        sprintReport.Result = sprintReportEditModel.Result;
                        sprintReport.UserComment = sprintReportEditModel.UserComment;
                        sprintReport.Status = sprintReportEditModel.Status.ToString();

                        db.SaveChanges();
                        break;
                }

            }

            return GetSprintReportItemById(sprintReport.SprintReportLineItemId);
        }
        public  SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId)
        {
            SprintReport sprintReport;
            using (var db = new ErpContext())
            {
                sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportLineItemId);

                if (sprintReport == null)
                {
                    throw new KeyNotFoundException("Error in finding sprintReportLineItem due to invalid Id.");
                }

                SprintReportEditModel sprintReportEditModel = new SprintReportEditModel()
                {
                    SprintReportLineItemId = sprintReport.SprintReportLineItemId,
                    TaskId = sprintReport.TaskId,
                    TaskDescription = sprintReport.TaskDescription,
                    CheckListItemId = sprintReport.CheckListItemId,
                    Description = sprintReport.Description,
                    ResultType = (CResultType) Enum.Parse(typeof(CResultType), sprintReport.ResultType,true),
                    Result = sprintReport.Result,
                    UserComment = sprintReport.UserComment,
                    ManagerComment = sprintReport.ManagerComment,
                    Approved = (SApproved) Enum.Parse(typeof(SApproved),sprintReport.Approved,true),
                    Status = (CStatus) Enum.Parse(typeof(CStatus), sprintReport.Status, true),
                    WorstCase = sprintReport.WorstCase,
                    BestCase = sprintReport.BestCase,
                    Score = sprintReport.Score,
                    SprintId = sprintReport.SprintId
                };

                return sprintReportEditModel;
            }
        }
       
        private  CheckListItemEditModel AddNewChecklistItemForTaskWithNoChecklist(string taskId)
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
                    CheckListType = ECheckListType.Task
                };

                return _checkListRepository.CreateOrUpdateCheckListInDb(dummyNewChecklistItem);
            }
        }



        private static string GetNextAvailableId()
            {
                using (var db = new ErpContext())
                {
                    var a = db.SprintReport
                        .Select(x => Convert.ToInt32(x.SprintReportLineItemId))
                        .DefaultIfEmpty(0)
                        .Max();
                    return Convert.ToString(a + 1);
                }
          
            }
        }
    

}