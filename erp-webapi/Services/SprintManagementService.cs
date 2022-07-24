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
        
        public static List<SprintEditModel> GetSprintsByProfileId(string profileId, string include = null)
        {
            List<string> sprintIds = GetSprintIdsForProfileId(profileId);
            List<SprintEditModel> sprints = new List<SprintEditModel>();
            
           sprintIds.ForEach(x =>
           {
              sprints.Add(GetSprintById(x)); 
           });

           return sprints;

        }
        
        public static SprintEditModel GetSprintById(string sprintId, string include = null)
        {
            SprintEditModel sprint =  GetSprintByIdFromDb(sprintId);
            sprint.Tasks = new List<TaskDetailEditModel>();
            if (include == "task")
            {
                using (var db = new ErpContext())
                {
                    List<string> taskDetailIds = db.TaskDetail
                        .Where(x => x.SprintId == sprintId)
                        .Select(x => x.TaskId)
                        .ToList();
                    taskDetailIds.ForEach(x =>
                        sprint.Tasks.Add(TaskManagementService.GetTaskById(x)));
                    
                }
            }

            return sprint;
        }

        private static List<string> GetSprintIdsForProfileId(string profileId, string include = null)
        {
            List<string> sprintIds;
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
            return AddOrUpdateSprintInDb(sprintEditModel);

        }
        
        private static SprintEditModel GetSprintByIdFromDb (string sprintId)
        {
            using (var db = new ErpContext())
            {
                
                Sprint existingSprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                // Case: TaskDetail does not exist
                if (existingSprint == null)
                    throw new KeyNotFoundException("Sprint id does not exist: " + sprintId);
                
                // Case: In case you have to update data received from db

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
                   Closed = existingSprint.Closed
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
                    if (sprint.Approved)
                    {
                        throw new ConstraintException("Sprint is froze, ask approver to close and plan new sprint");
                    }

                    
                    sprint.Description = sprintEditModel.Description;
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
                
                // Provisional score of task
                // TaskManagementService.UpdateProvisionalTaskScore(sprintId);
                //
                // List<int?> taskScores = db.TaskDetail
                //     .Where(x => x.SprintId == sprintId)
                //     .Select(x => x.Score)
                //     .ToList();
                //
                // // Provisional score of sprint
                // sprint.Score = taskScores.Sum();
                
                db.SaveChanges();
                
                // Unlinking tasks from sprint
                List<string> tasks = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.TaskId)
                    .ToList();
                
                tasks.ForEach(x =>
                {
                    TaskManagementService.RemoveTaskFromSprint(x);
                });

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

                if (!SprintReportManagementService.AllSprintReportLineItemsStatusNotNoChange(sprintId))
                {
                    throw new ConstraintException("Sprint report line items have status no change");
                }

                sprint.Status = SStatus.Reviewed.ToString();
                db.SaveChanges();

                // SprintReportManagementService.PublishActualScores(sprintId);

            }
            return GetSprintById(sprintId);
        }
        
        public static string CheckStatus(string sprintId)
        {
            SprintEditModel sprintEditModel = GetSprintById(sprintId);
            return sprintEditModel.Status.ToString().ToLower();
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
