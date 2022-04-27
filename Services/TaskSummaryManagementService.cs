using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Services
{
    public class TaskSummaryManagementService
    {
        public static TaskSummaryEditModel GetTaskSummaryById(string taskSummaryId, string include = null)
        {
            return GetTaskSummaryByIdFromDb(taskSummaryId);

        }
        
        public static List<TaskSummaryEditModel> GetAllTaskSummaryByTaskId(string taskId, string include = null)
        {
            List<string> taskSummaryIds =  GetTaskSummaryIdsForTaskId(taskId);
              
            List<TaskSummaryEditModel> taskSummaryList = new List<TaskSummaryEditModel>();
            taskSummaryIds.ForEach(x =>
            {
                taskSummaryList.Add(GetTaskSummaryById(x));
            });
            if (include == null)
            {
                return taskSummaryList;
            }
            if (include.Contains("allChildren"))
            {
                List<string> childTaskIds = TaskHierarchyManagementService
                    .GetDownStreamTaskIdsForTaskId(taskId); // includes parent taskId
                childTaskIds.Remove(taskId);
                List<string> childTaskSummaryIds = new List<string>();
                childTaskIds.ForEach(x => childTaskSummaryIds
                    .AddRange(GetTaskSummaryIdsForTaskId(x)));
                childTaskSummaryIds.ForEach(x =>
                {
                    taskSummaryList.Add(GetTaskSummaryById(x));
                });
                
            }
            return taskSummaryList;

        }
        
       
        private static List<string> GetTaskSummaryIdsForTaskId(string taskId, string include = null)
        {
            List<string> taskSummaryIds;
            using (var db = new ErpContext())
            {
                taskSummaryIds = db.TaskSummary
                    .Where(x => x.TaskId == taskId)
                    .Select(x => x.TaskSummaryId)
                    .ToList();
            }

            return taskSummaryIds;

        }
        
        

        private static TaskSummaryEditModel GetTaskSummaryByIdFromDb (string taskSummaryId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSummary existingTaskSummary = db.TaskSummary
                    .Include(x =>x.TaskDetail)
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
                    TaskScheduleId = existingTaskSummary.TaskScheduleId
                };

                return taskSummaryEditModel;
            }

        }
        
        public static TaskSummaryEditModel AddOrUpdateTaskSummary(TaskSummaryEditModel taskSummaryEditModel)
        {
            return AddOrUpdateTaskSummaryInDb(taskSummaryEditModel);

        }
        
        private static TaskSummaryEditModel AddOrUpdateTaskSummaryInDb(TaskSummaryEditModel taskSummaryEditModel)
        {
            TaskSummary taskSummary;
            
            using (var db = new ErpContext())
            {
                taskSummary = db.TaskSummary
                    .FirstOrDefault(x => x.TaskSummaryId == taskSummaryEditModel.TaskSummaryId);


                if (taskSummary != null) // update
                {
                    taskSummary.TaskId = taskSummaryEditModel.TaskId;
                    taskSummary.Date = taskSummaryEditModel.Date;
                    taskSummary.ExpectedHour = taskSummaryEditModel.ExpectedHour;
                    taskSummary.ExpectedOutput = taskSummaryEditModel.ExpectedOutput;
                    taskSummary.ActualHour = taskSummaryEditModel.ActualHour;
                    taskSummary.ActualOutput = taskSummaryEditModel.ActualOutput;
                    taskSummary.TaskScheduleId = taskSummaryEditModel.TaskScheduleId;
                    db.SaveChanges();
                }
                else
                {
                    taskSummary = new TaskSummary()
                    {
                        TaskSummaryId = GetNextAvailableId(),
                        TaskId = taskSummaryEditModel.TaskId,
                        Date = taskSummaryEditModel.Date,
                        ExpectedHour = taskSummaryEditModel.ExpectedHour,
                        ExpectedOutput = taskSummaryEditModel.ExpectedOutput,
                        ActualHour = taskSummaryEditModel.ActualHour,
                        ActualOutput = taskSummaryEditModel.ActualOutput,
                        TaskScheduleId = taskSummaryEditModel.TaskScheduleId
                    };
                    db.TaskSummary.Add(taskSummary);
                    db.SaveChanges();
                }
            }

            return GetTaskSummaryById(taskSummary.TaskSummaryId);
        }
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.TaskSummary
                    .Select(x => Convert.ToInt32(x.TaskSummaryId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public static void DeleteTaskSummary(string taskSummaryId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                TaskSummary existingTaskSummary = db.TaskSummary
                    .FirstOrDefault(x => x.TaskSummaryId == taskSummaryId);
                
                if (existingTaskSummary != null)
                {
                    
                    db.TaskSummary.Remove(existingTaskSummary);
                    db.SaveChanges();
                }


            }
        }

    }
}