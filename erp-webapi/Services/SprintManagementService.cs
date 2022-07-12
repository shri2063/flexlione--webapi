using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

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
                    return null;
                
                // Case: In case you have to update data received from db

                SprintEditModel sprintEditModel = new SprintEditModel()
                {
                   SprintId = existingSprint.SprintId,
                   Description = existingSprint.Description,
                   Owner = existingSprint.Owner,
                   FromDate = existingSprint.FromDate,
                   ToDate = existingSprint.ToDate,
                   Deliverable = existingSprint.Deliverable,
                   Delivered = existingSprint.Delivered,
                   ApproverId = existingSprint.ApproverId,
                   Approved = existingSprint.Approved
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
                        throw new ConstraintException("Sprint is freezed, ask approver to unapprove first");
                    }
                    
                    sprint.Description = sprintEditModel.Description;
                    sprint.Owner = sprintEditModel.Owner;
                    sprint.FromDate = sprintEditModel.FromDate;
                    sprint.ToDate = sprintEditModel.ToDate;
                    sprint.Deliverable = sprintEditModel.Deliverable;
                    sprint.Delivered = sprintEditModel.Delivered;
                    sprint.ApproverId = sprintEditModel.ApproverId;
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
                        Score = 0,
                        Deliverable = sprintEditModel.Deliverable,
                        Delivered = sprintEditModel.Delivered,
                        ApproverId = sprintEditModel.ApproverId,
                        Approved = false
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
                    
                    db.Sprint.Remove(existingSprint);
                    db.SaveChanges();
                }


            }
        }


        public static SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                if(sprint.ApproverId!=approverId && sprint.ApproverId!=null)
                {
                    throw new ArgumentException("Approver id is not eligible to approve the task");
                }

                sprint.Approved = true;
                db.SaveChanges();
            }
            
            return GetSprintById(sprintId);
        }
        
        public static SprintEditModel UnapproveSprint(string sprintId, string approverId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);

                if(sprint.ApproverId!=approverId && sprint.ApproverId!=null)
                {
                    throw new ArgumentException("Approver id is not eligible to unapprove the task");
                }
                
                sprint.Approved = false;
                db.SaveChanges();
            }
            
            return GetSprintById(sprintId);
        }

        public static void UpdateSprintScore(string sprintId, int? actualScore, int? bestScore, int? worstScore)
        {
            using (var db = new ErpContext())
            {
                Sprint sprint = db.Sprint
                    .FirstOrDefault(x=>x.SprintId==sprintId);

                if (actualScore < worstScore)
                {
                    if (sprint.Score > 0)
                    {
                        sprint.Score--;
                        db.SaveChanges();
                        return;
                    }
                }
                
                else if (actualScore > bestScore)
                {
                    throw new ArgumentException("invalid actual score");
                }
                
                sprint.Score++;
                db.SaveChanges();

            }
        }
        
        public static bool CheckApproved(string sprintId)
        {
            SprintEditModel sprintEditModel = GetSprintById(sprintId);
            return sprintEditModel.Approved;
        }
    }
}
