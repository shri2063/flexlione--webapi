using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Repository
{
    public class TaskSummaryRelationRepository: ITaskSummaryRelationRepository
    {
        public List<TaskSummaryEditModel> GetTaskSummaryIdsForTaskId(string taskId)
        {
            List<TaskSummaryEditModel> taskSummaryList = new List<TaskSummaryEditModel>();
            using (var db = new ErpContext())
            {

                List<TaskSummary> taskSummaries = db.TaskSummary
                    .Include(x => x.TaskDetail)
                    .Include(x => x.TaskSchedule)
                    .Where(x => x.TaskId == taskId)
                    .ToList();

                // Case: TaskDetail does not exist
                if (taskSummaries == null)
                    return null;

                taskSummaries.ForEach(x =>
                {
                    TaskSummaryEditModel taskSummaryEditModel = new TaskSummaryEditModel()
                    {
                        TaskSummaryId = x.TaskSummaryId,
                        Description = x.TaskDetail.Description,
                        TaskId = x.TaskId,
                        Date = x.Date,
                        ExpectedHour = x.ExpectedHour,
                        ExpectedOutput = x.ExpectedOutput,
                        ActualHour = x.ActualHour,
                        ActualOutput = x.ActualOutput,
                        TaskScheduleId = x.TaskScheduleId,
                        Stamp = x.Stamp,
                        Action = x.Action,
                        SystemHours = x.SystemHours,
                        TaskSchedule = new TaskShortScheduleEditModel()
                        {
                            TaskScheduleId = x.TaskSchedule.TaskScheduleId,
                            IsPlanned = x.TaskSchedule.IsPlanned,
                            StartHour = x.TaskSchedule.StartHour
                        },
                        Task = new TaskShortDetailEditModel()
                        {
                            TaskId = x.TaskDetail.TaskId,
                            Description = x.TaskDetail.Description,
                            Status = (EStatus)Enum.Parse(typeof(EStatus), x.TaskDetail.Status, true)
                        }
                    };
                    taskSummaryList.Add(taskSummaryEditModel);

                });

                return taskSummaryList;
            }
        }
    }
}