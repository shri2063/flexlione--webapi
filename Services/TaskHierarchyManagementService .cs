using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Services
{
    public class TaskHierarchyManagementService
    {

        public static TaskHierarchyEditModel GetTaskHierarchyByTaskId(string taskId, string include = null)
        {
            TaskHierarchyEditModel taskHierarchy = GetTaskHierarchyByIdFromDb(taskId);
            taskHierarchy.TotalHoursSpent = TaskSummaryManagementService
                .GetAllTaskSummaryByTaskId(taskId,"allChildren")
                .Sum(x => x.ActualHour);
            taskHierarchy.TotalEstimatedHours = TaskSummaryManagementService
                .GetAllTaskSummaryByTaskId(taskId,"allChildren")
                .Sum(x => x.ExpectedHour);

            if (include == null)
            {
                return taskHierarchy;
            }

            if (include.Contains("children"))
            {
                taskHierarchy.ChildrenTaskHierarchy = new List<TaskHierarchyEditModel>();
                TaskManagementService.GetChildTaskIdList(taskId)
                    .ForEach(x =>taskHierarchy.ChildrenTaskHierarchy
                        .Add(GetTaskHierarchyByTaskId(x)));
                return taskHierarchy;
            }

            return taskHierarchy;

        }
        
        public static List<TaskHierarchyEditModel>  UpdateTaskHierarchy(string taskId = null)
        {
            List<string> taskIdList;
            List<TaskHierarchyEditModel> updatedTaskHierarchy = new List<TaskHierarchyEditModel>();
            if (taskId == null) // Updating for all TaskIds
            {
                taskIdList = TaskManagementService.GetTaskIdList();
            }
            else
            {
                taskIdList = GetDownStreamTaskIdsForTaskId(taskId); // Updating for old task Id

                if (taskIdList.Count == 0)
                {
                    taskIdList.Add(taskId); // updating for new task Id
                }
            }
            
            taskIdList.ForEach(x =>
            {
                updatedTaskHierarchy.Add(UpdateTaskHierarchyInDb(x, GetUpStreamTaskIdsForTaskId(x)));
            });

            return updatedTaskHierarchy;
        }
        
        public static void DeleteTaskHierarchy(string taskId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                TaskHierarchy existingTaskHierarchy = db.TaskHierarchy
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                if (existingTaskHierarchy != null)
                {
                    
                    db.TaskHierarchy.Remove(existingTaskHierarchy);
                    db.SaveChanges();
                }


            }
        }
        private static TaskHierarchyEditModel GetTaskHierarchyByIdFromDb (string taskId)
        {
            using (var db = new ErpContext())
            {
                
                TaskHierarchy existingTaskHierarchy = db.TaskHierarchy
                    .Include(x => x.TaskDetail)
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                // Case: Task Schedule does not exist
                if (existingTaskHierarchy == null)
                {
                  throw  new Exception("Task id hierarchy does not exist " + taskId);
                }

                TaskHierarchyEditModel taskHierarchyEditModel = new TaskHierarchyEditModel()
                {
                    TaskHierarchyId = existingTaskHierarchy.TaskHierarchyId,
                    Description = existingTaskHierarchy.TaskDetail.Description,
                    TaskId = existingTaskHierarchy.TaskId,
                    TaskIds = existingTaskHierarchy.TaskIds
                    
                };

                return taskHierarchyEditModel;
            }
        }

        private static List<string> GetUpStreamTaskIdsForTaskId(string taskId)
        {
            List<string> taskIds = new List<string>();
            taskIds.Add(taskId);
            string currentTaskId = taskId;
            while (currentTaskId != "0")
            {
                currentTaskId = GetParentTaskForTaskId(currentTaskId);
                taskIds.Add(currentTaskId);
            }
            return taskIds;
           
        }
        
        public static List<string> GetDownStreamTaskIdsForTaskId(string taskId)
        {
           
            using (var db = new ErpContext())
            {
             return db.TaskHierarchy
                    .Where(x => x.TaskIds.Contains(taskId))
                    .Select(x => x.TaskId)
                    .ToList();
            }
        }

        private static TaskHierarchyEditModel UpdateTaskHierarchyInDb(string taskId, List<string> taskIds)
        {
            using (var db = new ErpContext())
            {
                TaskHierarchy taskHierarchy = db.TaskHierarchy
                    .FirstOrDefault(x => x.TaskId == taskId);


                if (taskHierarchy != null) // update
                {
                    taskHierarchy.TaskIds = taskIds;
                    db.SaveChanges();
                }
                else
                {
                    taskHierarchy = new TaskHierarchy()
                    {
                        TaskHierarchyId = GetNextAvailableId(),
                        TaskId = taskId,
                        TaskIds = taskIds

                    };
                    db.TaskHierarchy.Add(taskHierarchy);
                    db.SaveChanges();
                }

                return GetTaskHierarchyByTaskId(taskHierarchy.TaskId);
            }
        }

        private static string GetParentTaskForTaskId(string taskId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);

                if (task == null)
                {
                    throw new Exception("Task for " + taskId + " does not exist" );
                }

                return task.ParentTaskId;

            }
        }
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.TaskHierarchy
                    .Select(x => Convert.ToInt32(x.TaskHierarchyId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
       

    }
}