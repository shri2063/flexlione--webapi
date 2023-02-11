using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Repository
{
    public class TaskSummaryRepository: ITaskSummaryRepository
    {
        public  TaskSummaryEditModel GetTaskSummaryById(string taskSummaryId, string include = null)
        {
            if (taskSummaryId == null)
            {
                return null;
            }
            return GetTaskSummaryByIdFromDb(taskSummaryId);

        }

        
        private  TaskSummaryEditModel GetTaskSummaryByIdFromDb (string taskSummaryId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSummary existingTaskSummary = db.TaskSummary
                    .Include(x =>x.TaskDetail)
                    .Include(x=>x.TaskSchedule)
                    .FirstOrDefault(x => x.TaskSummaryId == taskSummaryId);
                
                // Case: TaskDetail does not exist
                if (existingTaskSummary == null)
                    return null;
                
                // Case: In case you have to update data received from db

                TaskSummaryEditModel taskSummaryEditModel = new TaskSummaryEditModel()
                {
                    TaskSummaryId = existingTaskSummary.TaskSummaryId,
                    Description = existingTaskSummary.TaskDetail.Description,
                    TaskId = existingTaskSummary.TaskId,
                    Date = existingTaskSummary.Date,
                    ExpectedHour = existingTaskSummary.ExpectedHour,
                    ExpectedOutput = existingTaskSummary.ExpectedOutput,
                    ActualHour = existingTaskSummary.ActualHour,
                    ActualOutput = existingTaskSummary.ActualOutput,
                    TaskScheduleId = existingTaskSummary.TaskScheduleId,
                    Stamp = existingTaskSummary.Stamp,
                    Action = existingTaskSummary.Action,
                    SystemHours = existingTaskSummary.SystemHours,
                    TaskSchedule = new TaskShortScheduleEditModel()
                    {
                        TaskScheduleId = existingTaskSummary.TaskSchedule.TaskScheduleId,
                        IsPlanned = existingTaskSummary.TaskSchedule.IsPlanned,
                        StartHour = existingTaskSummary.TaskSchedule.StartHour
                    },
                    Task = new TaskShortDetailEditModel()
                    {
                        TaskId = existingTaskSummary.TaskDetail.TaskId,
                        Description = existingTaskSummary.TaskDetail.Description,
                        Status = (EStatus) Enum.Parse(typeof(EStatus), existingTaskSummary.TaskDetail.Status, true)
                    }
                };

                return taskSummaryEditModel;
            }

        }
        
    }
}