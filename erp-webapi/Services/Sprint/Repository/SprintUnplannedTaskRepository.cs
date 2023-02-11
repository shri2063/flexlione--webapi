using System;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class SprintUnplannedTaskRepository: ISprintUnplannedTaskRepository
    {
        public SprintUnplannedTaskDataEditModel GetUnplannedTaskScoreData(string sprintId, string taskId)
        {
            SprintUnplannedTaskDataEditModel unplannedTaskData = new SprintUnplannedTaskDataEditModel();
            using (var dbSaved = new ErpContext())
            {
                SprintUnplannedTaskData unplannedTask = dbSaved.SprintUnplannedTaskData
                    .FirstOrDefault(y => y.TaskId == taskId && y.SprintId == sprintId);
                if (unplannedTask != null)
                {
                
                    unplannedTaskData.Id = unplannedTask.Id;
                    unplannedTaskData.SprintId = unplannedTask.SprintId;
                    unplannedTaskData.TaskId = unplannedTask.TaskId;
                    unplannedTaskData.RequestedHours = unplannedTask.RequestedHours;
                    unplannedTaskData.ApprovedHours = unplannedTask.ApprovedHours;
                    unplannedTaskData.ProfileId = unplannedTask.ProfileId;
                    unplannedTaskData.ScoreStatus =
                        (EUnplannedTaskStatus) Enum.Parse(typeof(EUnplannedTaskStatus), unplannedTask.ScoreStatus, true);
                }

            }

            return unplannedTaskData;
        }
        
        public SprintUnplannedTaskDataEditModel CreateOrUpdateSprintUnplannedTask(SprintUnplannedTaskDataEditModel unplannedTaskEdit )
        {
            
            using (var db = new ErpContext())
            {
                SprintUnplannedTaskData unplannedTask = db.SprintUnplannedTaskData
                    .FirstOrDefault(x => x.SprintId == unplannedTaskEdit.SprintId && x.TaskId == unplannedTaskEdit.TaskId);
                    

                if (unplannedTask != null) // update
                {
                    unplannedTask.ApprovedHours = unplannedTaskEdit.ApprovedHours ?? default(int);
                    unplannedTask.RequestedHours = unplannedTaskEdit.RequestedHours ?? default(int);
                    unplannedTask.ScoreStatus = unplannedTaskEdit.ScoreStatus.ToString();
                    unplannedTask.ProfileId = unplannedTaskEdit.ProfileId;

                    db.SaveChanges();
                }
                else // add new
                {
                    SprintUnplannedTaskData unplanned = new SprintUnplannedTaskData()
                    {
                        Id = GetNextAvailableUnplannedId(),
                        TaskId = unplannedTaskEdit.TaskId,
                        SprintId = unplannedTaskEdit.SprintId,
                        ApprovedHours = unplannedTaskEdit.ApprovedHours ?? default(int),
                        ProfileId = unplannedTaskEdit.ProfileId,
                        RequestedHours = unplannedTaskEdit.RequestedHours ?? default(int),
                        ScoreStatus = unplannedTaskEdit.ScoreStatus.ToString(),
                        
                    };
                    db.SprintUnplannedTaskData.Add(unplanned);
                    db.SaveChanges();
                }
            }
            
            return GetUnplannedTaskScoreData(unplannedTaskEdit.SprintId, unplannedTaskEdit.TaskId);
        }
        
        private static string GetNextAvailableUnplannedId()
         {
             using (var db = new ErpContext())
             {
                 var a = db.SprintUnplannedTaskData
                     .Select(x => Convert.ToInt32(x.Id))
                     .DefaultIfEmpty(0)
                     .Max();
                 return Convert.ToString(a + 1);
             }

         }
        
    }
}