using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Repository
{
    public class TaskScheduleRepository: ITaskScheduleRepository
    {
      
        public  TaskScheduleEditModel GetTaskScheduleByIdFromDb (string taskScheduleId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSchedule existingTaskSchedule = db.TaskSchedule
                    .Include(x => x.TaskDetail)
                    .Include(x => x.TaskSummary)
                    .FirstOrDefault(x => x.TaskScheduleId == taskScheduleId);
                
                // Case: Task Schedule does not exist
                if (existingTaskSchedule == null)
                {
                    return null;
                }
                    
                
                // Case: In case you have to update data received from db

                TaskScheduleEditModel taskScheduleEditModel = new TaskScheduleEditModel()
                {
                    TaskScheduleId = existingTaskSchedule.TaskScheduleId,
                    Description = existingTaskSchedule.TaskDetail.Description,
                    Owner = existingTaskSchedule.Owner,
                    TaskId = existingTaskSchedule.TaskId,
                    Date = existingTaskSchedule.Date,
                    StartHour = existingTaskSchedule.StartHour,
                    StopHour = existingTaskSchedule.StopHour,
                    StartMinute = existingTaskSchedule.StartMinute,
                    StopMinute = existingTaskSchedule.StopMinute,
                    IsPlanned = existingTaskSchedule.IsPlanned,
                    TaskSummaryId = existingTaskSchedule.TaskSummary == null ? "" :existingTaskSchedule.TaskSummary.TaskSummaryId
                };

                return taskScheduleEditModel;
            }
        }
    }
}