using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Services
{
    public class SprintManagementService
    {
        
        public static List<SprintEditModel> GetSprintsByProfileId(string profileId, List<string> include = null)
        {
            List<string> sprintIds = GetSprintIdsForProfileId(profileId);
            List<SprintEditModel> sprints = new List<SprintEditModel>();
            // [Check] Atleast one Sprint exisits

            if (sprintIds == null)
            {
                return null;
            }
            sprintIds.ForEach(x =>
           {
              sprints.Add(GetSprintById(x,new List<string>(include))); 
           });

            return sprints;

        }
        
        public static SprintEditModel GetSprintById(string sprintId, List<string> include = null)
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

        private static List<TaskDetailEditModel> GetPlannedTasksForSprint(string sprintId)
        {
            List<TaskDetailEditModel> plannedTasks = new List<TaskDetailEditModel>();
            List<string> taskDetailIds = TaskManagementService.GetTaskIdsForSprint(sprintId);
            if(taskDetailIds != null){ taskDetailIds.ForEach(x =>
                plannedTasks.Add(TaskManagementService.GetTaskById(x))) ;}

            return plannedTasks;
        }
        
        private static List<TaskDetailEditModel> GetUnPlannedTasksForSprint(string sprintId)
        {
            List<TaskDetailEditModel> unPlannedTasks = new List<TaskDetailEditModel>();
            List<string> plannedTaskDetailIds = TaskManagementService.GetTaskIdsForSprint(sprintId);
            var sprint = GetSprintById(sprintId);
            if (sprint == null)
            {
                return unPlannedTasks;
                
            }

            // WaterFall: get All Task Ids covered in Sprint Span
            var calenderTaskIds = (
                from s in 
                TaskScheduleManagementService.GetAllTaskScheduleByProfileIdAndDateRange(sprint.Owner, sprint.FromDate,
                sprint.ToDate)
                select s.TaskId)
                .Distinct()
                .ToList();
            
            var unPlannedTaskIds = new List<string>(calenderTaskIds);
            // Filter Task Ids  which have upstream in planned task ids
            foreach (var taskId in calenderTaskIds)
            {
                var upStreamTaskIds = (from s in
                        TaskHierarchyManagementService.GetTaskHierarchyByIdFromDb(taskId).ChildrenTaskIdList
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
                        TaskHierarchyManagementService.GetTaskHierarchyByIdFromDb(taskId).ChildrenTaskIdList
                    select s).ToList();
                upStreamTaskIds.RemoveAt(0);
                var commonTaskIds = upStreamTaskIds.Intersect(unPlannedTaskIds);

                if (commonTaskIds.Any())
                {
                    filteredUnPlannedTaskIds.Remove(taskId);
                }
            }
            
            
            unPlannedTaskIds.ForEach(x => unPlannedTasks.Add(TaskManagementService.GetTaskById(x)));
            return unPlannedTasks;
        }

        private static List<string> GetSprintIdsForProfileId(string profileId, string include = null)
        {
            List<string> sprintIds = new List<string>();
            using (var db = new ErpContext())
            {
                sprintIds = db.Sprint
                    .Where(x => x.Owner == profileId)
                    .Select(x => x.SprintId)
                    .ToList();
            }

            return sprintIds;

        }
        
        
        
        public static SprintEditModel AddOrUpdateSprint(SprintEditModel sprintEditModel)
        {
            // [Check]: Previous all sprints closed in case of new sprint
            if (GetSprintById(sprintEditModel.SprintId) == null)
            {
                List<SprintEditModel> sprints = GetSprintsByProfileId(sprintEditModel.Owner);

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
        
        private static SprintEditModel AddOrUpdateSprintInDb(SprintEditModel sprintEditModel)
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
                        SprintNo = 0,
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

            return GetSprintById(sprint.SprintId);
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
        
        public static void DeleteSprint(string sprintId)
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

        public static SprintEditModel RequestForApproval(string sprintId, string userId)
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

                if (sprint.Status != SStatus.Planning.ToString())
                {
                    throw new ConstraintException("status is not planning, hence request for approval can't be made");
                }
                
                if (TotalExpectedHours(sprintId) > 6*ValidSprintDays(sprintId))
                {
                    throw new ConstraintException("expected hours more then total sprint time");
                }

                sprint.Status = SStatus.RequestForApproval.ToString();
                db.SaveChanges();
            }

            return GetSprintById(sprintId);
        }

        private static int TotalExpectedHours(string sprintId)
        {
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

        public static SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to approve the sprint");
                }

                if (sprint.Status != SStatus.RequestForApproval.ToString())
                {
                    throw new ConstraintException("Sprint not requested for approval hence can't be approved");
                }

                sprint.Status = SStatus.Approved.ToString();
                sprint.Approved = true;
                db.SaveChanges();

                SprintReportManagementService.AddSprintReportLineItem(sprint.SprintId);
            }
            
            return GetSprintById(sprintId);
        }
        
        public static SprintEditModel RequestForClosure(string sprintId, string userId)
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
        
        public static SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to close the sprint");
                }

                if (sprint.Status != SStatus.RequestForClosure.ToString())
                {
                    throw new ConstraintException("Sprint not requested for closure hence can't be closed");
                }

                sprint.Status = SStatus.Closed.ToString();
                sprint.Closed = true;
                
                // [Check]: If closed early then Sprint closing date needs to be updated
                sprint.ToDate = DateTime.Today;
                
                // Provisional score of task
                List<string> taskIds = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.TaskId)
                    .ToList();
                
                taskIds.ForEach(taskId =>
                {
                    TaskManagementService.UpdateProvisionalTaskScore(taskId);
                });
                
                List<int?> taskScores = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.Score)
                    .ToList();
                
                // Provisional score of sprint
                sprint.Score = taskScores.Sum();
                
                db.SaveChanges();
                
                // Unlinking tasks from sprint
                taskIds.ForEach(x =>
                {
                    TaskManagementService.RemoveTaskFromSprint(x);
                });
                
                // Provisional Score Sprint Report
                SprintReportManagementService.UpdateProvisionalScoreInSprintReport(sprintId);

            }
            
            return GetSprintById(sprintId);
        }

        public static SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                if(!ProfileManagementService.CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to review the sprint");
                }

                if (sprint.Status != SStatus.Closed.ToString())
                {
                    throw new ConstraintException("Sprint cannot be reviewed as status is not closed");
                }

                sprint.Status = SStatus.Reviewed.ToString();

                // Actual Score Sprint Report
                SprintReportManagementService.PublishActualScoresForSprintReport(sprintId);

                List<string> taskIds = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.TaskId)
                    .ToList();
                
                // Actual Score Task
                taskIds.ForEach(taskId =>
                {
                    TaskManagementService.PublishActualScoresForTask(taskId, sprintId);
                });

                // Actual score of sprint
                List<int?> taskScores = new List<int?>();
                taskIds.ForEach(taskId =>
                {
                    taskScores = db.TaskDetail
                        .Where(x => x.TaskId == taskId)
                        .Select(x => x.Score)
                        .ToList();
                });
                
                sprint.Score = taskScores.Sum();
                
                db.SaveChanges();



            }
            return GetSprintById(sprintId);
        }
        
        public static SStatus CheckStatus(string sprintId)
        {
            SprintEditModel sprintEditModel = GetSprintById(sprintId);
            return sprintEditModel.Status;
        }

        public static bool CheckApproved(string sprintId)
        {
            using (var db = new ErpContext())
            {
                var status = db.Sprint
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.Approved)
                    .ToString();

                if (status=="true")
                {
                    return true;
                }

                return false;
            }
        }
        
        public static bool CheckClosed(string sprintId)
        {
            using (var db = new ErpContext())
            {
                var status = db.Sprint
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.Closed)
                    .Single();

                if (status)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
