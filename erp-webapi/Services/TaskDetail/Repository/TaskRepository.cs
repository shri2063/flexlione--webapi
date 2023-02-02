using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;

namespace flexli_erp_webapi.Services.TaskSearch
{
    public class TaskRepository : ITaskRepository
    {
        
        public TaskDetailEditModel GetTaskById(string taskId)
        {
            using (var db = new ErpContext())
            {

                DataModels.TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);

                // Case: TaskDetail does not exist
                if (existingTask == null)
                    return null;

                // Case: Status is not mentioned.
                // Its a check, ideally it should never be null
                // Make it Yet To Start
                if (existingTask.Status == null)
                {
                    existingTask.Status = EStatus.yettostart.ToString();
                }

                TaskDetailEditModel taskDetailEditModel = new TaskDetailEditModel()
                {
                    TaskId = existingTask.TaskId,
                    ParentTaskId = existingTask.ParentTaskId,
                    CreatedAt = existingTask.CreatedAt,
                    Deadline = existingTask.Deadline,
                    CreatedBy = existingTask.CreatedBy,
                    AssignedTo = existingTask.AssignedTo,
                    Status = (EStatus)Enum.Parse(typeof(EStatus), existingTask.Status, true),
                    Description = existingTask.Description,
                    PositionAfter = existingTask.PositionAfter,
                    Rank = existingTask.Rank,
                    SprintId = existingTask.SprintId,
                    IsRemoved = existingTask.IsRemoved,
                    ExpectedHours = existingTask.ExpectedHours,
                    Score = existingTask.Score,
                    AcceptanceCriteria = existingTask.AcceptanceCriteria,
                    EditedAt = existingTask.EditedAt,
                    ActualHours = existingTask.ActualHours,
                };

                return taskDetailEditModel;
            }

        }

        
        public  TaskDetailEditModel CreateOrUpdateTask(TaskDetailEditModel taskDetailEditModel)
        {
           DataModels.TaskDetail task;
         
           using (var db = new ErpContext())
           {
               task = db.TaskDetail
                   .FirstOrDefault(x => x.TaskId == taskDetailEditModel.TaskId);

               if (task != null) // update
               {
                  
                   task.ParentTaskId = taskDetailEditModel.ParentTaskId;
                   task.CreatedBy = taskDetailEditModel.CreatedBy.ToLower();
                   task.Status = taskDetailEditModel.Status.ToString().ToLower();
                   task.Description = taskDetailEditModel.Description;
                   task.AssignedTo = taskDetailEditModel.AssignedTo.ToLower();
                   task.Deadline = taskDetailEditModel.Deadline;
                   task.ExpectedHours = taskDetailEditModel.ExpectedHours;
                   task.EditedAt = DateTime.Now;
                   task.AcceptanceCriteria = taskDetailEditModel.AcceptanceCriteria ?? 0;
                   task.PositionAfter = taskDetailEditModel.PositionAfter ?? task.PositionAfter;

                   


                   db.SaveChanges();
               }
               else
               {
                   var newTaskId = GetNextAvailableId();
                   task = new DataModels.TaskDetail
                   {
                       TaskId = newTaskId,
                       ParentTaskId = taskDetailEditModel.ParentTaskId,
                       CreatedAt = DateTime.Now,
                       CreatedBy = taskDetailEditModel.CreatedBy,
                       Status = taskDetailEditModel.Status.ToString().ToLower(),
                       Description = taskDetailEditModel.Description,
                       AssignedTo = taskDetailEditModel.AssignedTo,
                       Deadline = taskDetailEditModel.Deadline,
                       EditedAt = DateTime.Now,
                       ExpectedHours = taskDetailEditModel.ExpectedHours,
                       IsRemoved = false,
                       AcceptanceCriteria = taskDetailEditModel.AcceptanceCriteria ?? 0,
                       PositionAfter = taskDetailEditModel.PositionAfter
                        
                   };
                   db.TaskDetail.Add(task);
                   db.SaveChanges();
                   
               }
               
           }
           // Update Task Hierarchy
           //_taskHierarchyRelationRepository.UpdateTaskHierarchy(task.TaskId);
           

           var createdTask =  GetTaskById(task.TaskId);
           return createdTask;

        }
        
        
        public  bool DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
              
                // Get Selected TasK
                DataModels.TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                db.TaskDetail.Remove(existingTask); 
                db.SaveChanges();
            }

            return true;

            
        }

        private string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.TaskDetail
                    .Select(x => Convert.ToInt32(x.TaskId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }

        }

        public  List<string> GetChildTaskIdList(string parentTaskId)
        {
            using (var db = new ErpContext())
            {
                var taskIds =  db.TaskDetail
                    .Where(x => x.ParentTaskId == parentTaskId)
                    .Select(t => t.TaskId)
                    .ToList();
                return taskIds;
            }
        }

        public  List<TaskShortDetailEditModel> GetTaskIdList(string parentTaskId = null, int? pageIndex = null,
            int? pageSize = null)
        {
            if (pageIndex != null && pageSize != null)
            {
                return GetTaskIdListPageForParentTaskId(parentTaskId, (int) pageIndex, (int) pageSize);
            }
            
            using (var db = new ErpContext())
            {
                if (parentTaskId == null)
                {
                    return db.TaskDetail
                        .Select(t => new TaskShortDetailEditModel()
                        {
                            TaskId = t.TaskId,
                            Description = t.Description,
                            Status = (EStatus) Enum.Parse(typeof(EStatus), t.Status, true)
                        })
                        .ToList();
                }
                return db.TaskDetail
                    .Where(t => t.ParentTaskId == parentTaskId)
                    .Select(t => new TaskShortDetailEditModel()
                    {
                        TaskId = t.TaskId,
                        Description = t.Description
                    })
                    .ToList();
                
            }
        }

        public void UpdateEditedAtTimeStamp(string taskId)
        {
            using (var db = new ErpContext())
            {
              
                // Get Selected TasK
                DataModels.TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (existingTask != null)
                {
                    existingTask.EditedAt = DateTime.Now;
                }
                db.SaveChanges();
            }
        }


        private static List<TaskShortDetailEditModel> GetTaskIdListPageForParentTaskId(string parentTaskId, int pageIndex, int pageSize)
        {
            using (var db = new ErpContext())
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                // skip take logic
                List<TaskShortDetailEditModel> taskShortDetailEditModels;
                    
                if (parentTaskId == null)
                {
                    taskShortDetailEditModels = db.TaskDetail
                        .Select(t => new TaskShortDetailEditModel()
                        {
                            TaskId = t.TaskId,
                            Description = t.Description,
                            Status = (EStatus) Enum.Parse(typeof(EStatus), t.Status, true)
                        })
                        .OrderByDescending(x=>Convert.ToInt32(x.TaskId))
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    taskShortDetailEditModels = db.TaskDetail
                        .Where(t => t.ParentTaskId == parentTaskId)
                        .Select(t => new TaskShortDetailEditModel()
                        {
                            TaskId = t.TaskId,
                            Description = t.Description
                        })
                        .OrderByDescending(t=>Convert.ToInt32(t.TaskId))
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                if (taskShortDetailEditModels.Count == 0)
                {
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                }

                return taskShortDetailEditModels;

            }
        }
        
        
          
        // Removed task will not be shown in Web App until forced
        public  bool RemoveTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected TasK
                DataModels.TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
             
                if (existingTask != null)
                {
                    existingTask.IsRemoved = true;
                    db.SaveChanges();
                }

            }

            return true;
        }

    }

}