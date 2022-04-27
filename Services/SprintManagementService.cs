using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;

namespace m_sort_server.Services
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
                   Delivered = existingSprint.Delivered
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
                    sprint.Owner = sprintEditModel.Owner;
                    sprint.FromDate = sprintEditModel.FromDate;
                    sprint.ToDate = sprintEditModel.ToDate;
                    sprint.Deliverable = sprintEditModel.Deliverable;
                    sprint.Delivered = sprintEditModel.Delivered;
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
                        Deliverable = sprintEditModel.Deliverable,
                        Delivered = sprintEditModel.Delivered
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
    }
}
