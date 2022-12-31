using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using m_sort_server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Repository
{
    public class TaskHierarchyRelationRepository: ITaskHierarchyRelationRepository

    {
        private readonly ITaskRepository _taskRepository;
       
        public TaskHierarchyRelationRepository(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        
          
        }
        
        
        public  void  UpdateTaskHierarchy(string taskId)
        {
            var upStreamTaskIds = GetUpStreamTaskIdsForTaskId(taskId);
            UpdateTaskHierarchyInDb(taskId, upStreamTaskIds);

        }

  

        public  void DeleteTaskHierarchy(string taskId)
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
        public  TaskHierarchyEditModel GetTaskHierarchyByTaskIdFromDb(string taskId)
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
                    ChildrenTaskIdList = existingTaskHierarchy.TaskIds
                    
                };

                return taskHierarchyEditModel;
            }
        }
        
        private  List<string> GetUpStreamTaskIdsForTaskId(string taskId)
        {
            List<string> taskIds = new List<string>();
            taskIds.Add(taskId);
            string currentTaskId = taskId;
            while (currentTaskId != "0")
            {
                currentTaskId = _taskRepository.GetTaskById(currentTaskId).ParentTaskId;
                taskIds.Add(currentTaskId);
            }
            return taskIds;
           
        }
        
        public  List<string> GetDownStreamTaskIdsForTaskId(string taskId)
        {
           
            using (var db = new ErpContext())
            {
                return db.TaskHierarchy
                    .Where(x => x.TaskIds.Contains(taskId))
                    .Select(x => x.TaskId)
                    .ToList();
            }
        }
        
        private  TaskHierarchyEditModel UpdateTaskHierarchyInDb(string taskId, List<string> taskIds)
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

                return GetTaskHierarchyByTaskIdFromDb(taskHierarchy.TaskId);
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