using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m_sort_server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Services
{
    public class SprintManagementService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly ISprintRelationRepository _sprintRelationRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private readonly ITaskScheduleRelationRepository _taskScheduleRelationRepository;
        public SprintManagementService(ITaskRepository taskRepository, ISprintRepository sprintRepository, 
            ITaskRelationRepository taskRelationRepository, ISprintRelationRepository sprintRelationRepository, ISprintReportRepository sprintReportRepository,
            ITaskHierarchyRelationRepository taskHierarchyRelationRepository, ITaskScheduleRelationRepository taskScheduleRelationRepository)
        {
            _taskRepository = taskRepository;
            _sprintRepository = sprintRepository;
            _taskRelationRepository = taskRelationRepository;
            _sprintRelationRepository = sprintRelationRepository;
            _sprintReportRepository = sprintReportRepository;
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
            _taskScheduleRelationRepository = taskScheduleRelationRepository;
        }



        public List<SprintEditModel> GetSprintsByProfileId(string profileId, List<String> include,
            int? pageIndex = null, int? pageSize = null)
        {
            var sprints = _sprintRelationRepository.GetSprintsForProfileId(profileId,  pageIndex, pageSize);
            
            sprints.ForEach(x =>
            {
                if (include.Contains("plannedTask"))
                {
                    x.PlannedTasks = GetPlannedTasksForSprint(x.SprintId);
                }
            
                if (include.Contains("unPlannedTask"))
                {
                    x.UnPlannedTasks = GetUnPlannedTasksForSprint(x.SprintId);
                }
            });
            return sprints;
        }
        
        public  SprintEditModel GetSprintById(string sprintId, List<string> include = null)
        {
            SprintEditModel sprint =  GetSprintByIdFromDb(sprintId);
            if (sprint == null)
            {
                return null;
            }

            if (include == null)
            {
                return sprint;
            }
            sprint.PlannedTasks = new List<TaskDetailEditModel>();
            if (include.Contains("plannedTask"))
            {
                sprint.PlannedTasks = GetPlannedTasksForSprint(sprintId);
            }
            
            if (include.Contains("unPlannedTask"))
            {
                sprint.UnPlannedTasks = GetUnPlannedTasksForSprint(sprintId);
            }

            return sprint;
        }

        private  List<TaskDetailEditModel> GetPlannedTasksForSprint(string sprintId)
        {
            List<TaskDetailEditModel> plannedTasks = new List<TaskDetailEditModel>();
            List<string> taskDetailIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
            if(taskDetailIds != null){ taskDetailIds.ForEach(x =>
                plannedTasks.Add(_taskRepository.GetTaskById(x))) ;}

            return plannedTasks;
        }
        
        private  List<TaskDetailEditModel> GetUnPlannedTasksForSprint(string sprintId)
        {
            List<TaskDetailEditModel> unPlannedTasks = new List<TaskDetailEditModel>();
            List<string> plannedTaskDetailIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
            var sprint = GetSprintById(sprintId);
            if (sprint == null)
            {
                return unPlannedTasks;
                
            }

            // WaterFall: get All Task Ids covered in Sprint Span
            var calenderTaskIds = (
                from s in 
                _taskScheduleRelationRepository.GetAllTaskScheduleByProfileIdAndDateRange(sprint.Owner, sprint.FromDate,
                sprint.ToDate)
                select s.TaskId)
                .Distinct()
                .ToList();
            
            var unPlannedTaskIds = new List<string>(calenderTaskIds);
            // Filter Task Ids  which have upstream in planned task ids
            foreach (var taskId in calenderTaskIds)
            {
                var upStreamTaskIds = (from s in
                        _taskHierarchyRelationRepository.GetTaskHierarchyByTaskIdFromDb(taskId).ChildrenTaskIdList
                    select s).ToList();
                
                var commonTaskIds = upStreamTaskIds.Intersect(plannedTaskDetailIds).ToList();

                if (commonTaskIds.Any())
                {
                     unPlannedTaskIds.Remove(taskId);
                }
            }
            
            // Filter Task Schedules which have upstream in another unplanned task schedules
            var filteredUnPlannedTaskIds = new List<string>(unPlannedTaskIds);
            
            foreach (var taskId in unPlannedTaskIds)
            {
                var upStreamTaskIds = (from s in
                        _taskHierarchyRelationRepository.GetTaskHierarchyByTaskIdFromDb(taskId).ChildrenTaskIdList
                    select s).ToList();
                upStreamTaskIds.RemoveAt(0);
                var commonTaskIds = upStreamTaskIds.Intersect(unPlannedTaskIds);

                if (commonTaskIds.Any())
                {
                    filteredUnPlannedTaskIds.Remove(taskId);
                }
            }
            
            
            unPlannedTaskIds.ForEach(x => unPlannedTasks.Add(_taskRepository.GetTaskById(x)));
            return unPlannedTasks;
        }

       
        
        
        public  SprintEditModel AddOrUpdateSprint(SprintEditModel sprintEditModel)
        {
            // [Check]: Previous all sprints closed in case of new sprint
            if (GetSprintById(sprintEditModel.SprintId) == null)
            {
                List<SprintEditModel> sprints = _sprintRelationRepository.GetSprintsForProfileId(sprintEditModel.Owner);

                if (sprints.Count > 0)
                {
                    var openSprints = sprints.FindAll(x => (!x.Closed))
                        .Count;
                    if (openSprints > 0)
                    {
                        throw new ConstraintException("All Previous sprints need to be closed");
                    }
                }

            }
              
            
            
            // [Check]: Sprint is in planning state in case of already created sprint

            if (GetSprintById(sprintEditModel.SprintId) != null)
            {
              if(GetSprintById(sprintEditModel.SprintId).Status != SStatus.Planning)
                  
                throw new ConstraintException("Sprint  can be updated only in planning stage ");
            }
            
            return AddOrUpdateSprintInDb(sprintEditModel);

        }
        
       
        
        private static SprintEditModel GetSprintByIdFromDb (string sprintId)
        {
            using (var db = new ErpContext())
            {
                
                Sprint existingSprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                // [Check]: Sprint does not exist
                if (existingSprint == null)
                {
                    return null;
                }
                    
                
               

                SprintEditModel sprintEditModel = new SprintEditModel()
                {
                   SprintId = existingSprint.SprintId,
                   Description = existingSprint.Description,
                   Owner = existingSprint.Owner,
                   FromDate = existingSprint.FromDate,
                   ToDate = existingSprint.ToDate,
                   Score = existingSprint.Score,
                   Status = (SStatus) Enum.Parse(typeof(SStatus), existingSprint.Status, true),
                   Approved = existingSprint.Approved,
                   Closed = existingSprint.Closed,
                   SprintNo = existingSprint.SprintNo
                };

                return sprintEditModel;
            }

        }
        
        private  SprintEditModel AddOrUpdateSprintInDb(SprintEditModel sprintEditModel)
        {
            Sprint sprint;
            
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintEditModel.SprintId);


                if (sprint != null) // update
                {
                    
                    sprint.Description = sprintEditModel.Description;
                    sprint.SprintNo = sprintEditModel.SprintNo;
                    sprint.Owner = sprintEditModel.Owner;
                    sprint.FromDate = sprintEditModel.FromDate;
                    sprint.ToDate = sprintEditModel.ToDate;
                    sprint.Score = sprintEditModel.Score;
                    db.SaveChanges();
                }
                else
                {
                    sprint = new Sprint()
                    {
                        SprintId = GetNextAvailableId(),
                        SprintNo = GetNewSprintNo(sprintEditModel),
                        Description = sprintEditModel.Description,
                        Owner = sprintEditModel.Owner,
                        FromDate = sprintEditModel.FromDate,
                        ToDate = sprintEditModel.ToDate,
                        Status = SStatus.Planning.ToString(),
                        Score = 0,
                        Approved = false,
                        Closed = false
                    };
                    db.Sprint.Add(sprint);
                    db.SaveChanges();
                }
            }

            return _sprintRepository.GetSprintById(sprint.SprintId);
        }

        private static decimal GetNewSprintNo(SprintEditModel sprintEditModel)
        {
            using (var db = new ErpContext())
            {
                // Max sprint running for an owner if No sprint yet, default = 1
                var sprintNo = db.Sprint
                    .Where(x=>x.Owner==sprintEditModel.Owner)
                    .Select(x => Convert.ToDecimal(x.SprintNo))
                    .DefaultIfEmpty(1)
                    .Max();

                // sprint with sprintNo.
                var prevSprint = db.Sprint
                    .FirstOrDefault(x => Convert.ToDecimal(x.SprintNo) == sprintNo);
                
                // if prevSprint doesn't exist then return with 1
                if (prevSprint == null)
                {
                    return sprintNo;
                }

                // [Check]: Condition for main sprint
                // Assign +1 Number.
                if (sprintEditModel.FromDate > prevSprint.ToDate &&
                    sprintEditModel.ToDate.Subtract(sprintEditModel.FromDate).Days>=10)
                {
                    return Math.Truncate(sprintNo) + 1;
                }
                        
                // [Check]: Sprints with conditions other than main sprint are sub sprints
                // Assign +0.1 Number
                return sprintNo + Convert.ToDecimal(0.1);
            }
        }

        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.Sprint
                    .Select(x => Convert.ToInt32(x.SprintId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public  void DeleteSprint(string sprintId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                Sprint existingSprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
               // [Check]: Sprint is in planning state 
                if (existingSprint != null)
                {
                    if (existingSprint.Status != SStatus.Planning.ToString())
                    {
                        throw new ConstraintException("Cannot delete the sprint, status is not planning");
                    }
                    
                    db.Sprint.Remove(existingSprint);
                    db.SaveChanges();
                    
                }


            }
        }

        public  SprintEditModel RequestForApproval(string sprintId, string userId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId && x.Owner == userId);
                
                // [check] : if sprint or user not exist
                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint Id or User Id does not exist");
                }

                // [check] : if status is not planning
                if (sprint.Status != SStatus.Planning.ToString())
                {
                    throw new ConstraintException("status is not planning, hence request for approval can't be made");
                }
                
                // [check] : total expected hours of sprint should not be more than 6 hours * working days between sprint
                if (TotalExpectedHours(sprintId) > 6*ValidSprintDays(sprintId))
                {
                    throw new ConstraintException("expected hours more then total sprint time");
                }

                sprint.Status = SStatus.RequestForApproval.ToString();
                db.SaveChanges();
            }

            return _sprintRepository.GetSprintById(sprintId);
        }

        private static int TotalExpectedHours(string sprintId)
        {
            // calculate total expected hours of all tasks in the sprint
            using (var db = new ErpContext())
            {
                List<Decimal?> expectedHours = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.ExpectedHours)
                    .ToList();

                return Convert.ToInt32(expectedHours.Sum());
            }
        }

        private static int ValidSprintDays(string sprintId)
        {
            // Calculate monday to friday working days in sprint
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);

                DateTime to = sprint.ToDate;
                DateTime from = sprint.FromDate;
                
                if (to < from)
                    throw new ArgumentException("To cannot be smaller than from.", nameof(to));

                int n = 0;
                DateTime nextDate = from;
                while(nextDate <= to.Date)
                {
                    if (nextDate.DayOfWeek != DayOfWeek.Saturday && nextDate.DayOfWeek != DayOfWeek.Sunday)
                        n++;
                    nextDate = nextDate.AddDays(1);
                }

                return n;
            }
        }

        public  SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                // [check] : validate manager
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to approve the sprint");
                }

                // [check] : if status is valid
                if (sprint.Status != SStatus.RequestForApproval.ToString())
                {
                    throw new ConstraintException("Sprint not requested for approval hence can't be approved");
                }
                
                // Add entries in sprint report before changing status so that error is thrown before approved
                _sprintReportRepository.AddSprintReportLineItem(sprint.SprintId);
                
                sprint.Status = SStatus.Approved.ToString();
                sprint.Approved = true;
                db.SaveChanges();
            }
            
            return GetSprintById(sprintId);
        }
        
        public  SprintEditModel RequestForClosure(string sprintId, string userId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId && x.Owner == userId);

                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint Id or User Id does not exist");
                }

                if (sprint.Status != SStatus.Approved.ToString())
                {
                    throw new ConstraintException("status is not approved, hence request for closure can't be made");
                }

                sprint.Status = SStatus.RequestForClosure.ToString();
                db.SaveChanges();
            }

            return GetSprintById(sprintId);
        }
        
        public  SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                if (sprint == null)
                {
                    throw new ConstraintException("Sprint Id does not exist" + sprintId);
                }
                sprint.Score = 0;
                
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to close the sprint");
                }

                if (sprint.Status != SStatus.RequestForClosure.ToString())
                {
                    throw new ConstraintException("Sprint not requested for closure hence can't be closed");
                }

                var taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
                
                
                // [Action] - Update Provisional task Score
                taskIds.ForEach(x =>
                {
                    sprint.Score = sprint.Score + CalculateTaskScore(x,sprintId);
                });
                
                sprint.Status = SStatus.Closed.ToString();
                sprint.Closed = true;
                // [Check]: If closed early then Sprint closing date needs to be updated
                sprint.ToDate = DateTime.Today;
                db.SaveChanges();
                // [Action] - remove task link to sprint
                var removedTask = new List<string>();
                try
                {
                    
                    taskIds.ForEach(x =>
                    {
                        _taskRelationRepository.RemoveTaskFromSprint(x);
                        removedTask.Add(x);
                    });
                }
                catch (Exception e)
                {
                    sprint.Status = SStatus.RequestForClosure.ToString();
                    sprint.Closed = false;
                    db.SaveChanges();
                    removedTask.ForEach(x => _taskRelationRepository.LinkTaskToSprint(x,sprint.SprintId));
                    throw new ConstraintException(e.Message);
                }
                
                
                // Provisional Score Sprint Report
                SprintReportManagementService.UpdateProvisionalScoreInSprintReport(sprintId);


            }
            
            return GetSprintById(sprintId);
        }

         public  int CalculateTaskScore(string taskId, string sprintId)
        {

            TaskDetailEditModel task =  _taskRepository.GetTaskById(taskId);
            List<SprintReportEditModel>
                    sprintReportLineItems = _sprintReportRepository.GetSprintReportForSprint(sprintId)
                        .FindAll(x => x.TaskId == taskId);
                    
                int complete = 0;
                int completeEssential = 0;
                int essential = 0;
            
                    
                sprintReportLineItems.ForEach(sprintReportLineItem =>
                {
                    
                    if (sprintReportLineItem.Essential)
                        essential++;

                    if (sprintReportLineItem.Essential && sprintReportLineItem.Status == CStatus.Completed && sprintReportLineItem.Approved != SApproved.False)
                    {
                        if(sprintReportLineItem.ResultType==CResultType.Numeric && Convert.ToInt32(sprintReportLineItem.Result)>=sprintReportLineItem.WorstCase && Convert.ToInt32(sprintReportLineItem.Result) <= sprintReportLineItem.BestCase)
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.Boolean && sprintReportLineItem.Result == "true")
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.File && sprintReportLineItem.Result != null)
                        {
                            complete++;
                            completeEssential++;
                        }
                    }
                        
                    else if (sprintReportLineItem.Status == CStatus.Completed && sprintReportLineItem.Approved != SApproved.False)
                    {
                        if(sprintReportLineItem.ResultType==CResultType.Numeric && Convert.ToInt32(sprintReportLineItem.Result)>=sprintReportLineItem.WorstCase && Convert.ToInt32(sprintReportLineItem.Result) <= sprintReportLineItem.BestCase)
                        {
                            complete++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.Boolean && sprintReportLineItem.Result == "true")
                        {
                            complete++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.File && sprintReportLineItem.Result != null)
                        {
                            complete++;
                        }
                    }

                });
                    
                if (completeEssential < essential || complete < task.AcceptanceCriteria)
                   return 0;

                return Convert.ToInt32(task.ExpectedHours / 3);
        }
        public  SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint does not exist: " + sprintId);
                }
                sprint.Score = 0;
                
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to review the sprint");
                }

                if (sprint.Status != SStatus.Closed.ToString())
                {
                    throw new ConstraintException("Sprint cannot be reviewed as status is not closed");
                }
                var taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
                
                
                // [Action] - Update Actual task Score
                taskIds.ForEach(x =>
                {
                    sprint.Score = sprint.Score + CalculateTaskScore(x,sprintId);
                });

                sprint.Status = SStatus.Reviewed.ToString();

                db.SaveChanges();



            }
            return GetSprintById(sprintId);
        }
        
        public  SStatus CheckStatus(string sprintId)
        {
            SprintEditModel sprintEditModel = _sprintRepository.GetSprintById(sprintId);
            return sprintEditModel.Status;
        }

        
        
        
    }
}
