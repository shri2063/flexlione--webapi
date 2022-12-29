using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class TaskRepository : ITaskRepository
    {
        public TaskDetailEditModel CreateOrUpdateTask(TaskDetailEditModel taskDetailEditModel)
        {
            TaskDetail task;
            if (taskDetailEditModel.Deadline == null)
            {
                taskDetailEditModel.Deadline = DateTime.MaxValue;
            }

            using (var db = new ErpContext())
            {
                task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskDetailEditModel.TaskId);

                if (task != null) // update
                {

                    if (task.AssignedTo != taskDetailEditModel.AssignedTo
                        && task.SprintId != null)
                    {
                        throw new Exception("Assignee cannot be changed. Already allocated in sprint");
                    }

                    task.ParentTaskId = taskDetailEditModel.ParentTaskId;
                    task.CreatedBy = taskDetailEditModel.CreatedBy.ToLower();
                    task.Status = taskDetailEditModel.Status.ToString().ToLower();
                    task.Description = taskDetailEditModel.Description;
                    task.AssignedTo = taskDetailEditModel.AssignedTo.ToLower();
                    task.Deadline = taskDetailEditModel.Deadline;
                    task.ExpectedHours = taskDetailEditModel.ExpectedHours;
                    task.EditedAt = DateTime.Now;
                    task.AcceptanceCriteria = taskDetailEditModel.AcceptanceCriteria ?? 0;

                    //[Action] If Sprint status = planning or Sprint not allocated then you can change acceptance criteria
                    if (task.SprintId != null ? task.Status == SStatus.Planning.ToString() : true)
                    {
                        task.AcceptanceCriteria = taskDetailEditModel.AcceptanceCriteria;
                    }

                    db.SaveChanges();
                }
                else
                {
                    var dateTime = DateTime.Now;
                    task = new TaskDetail
                    {
                        TaskId = GetNextAvailableId(),
                        ParentTaskId = taskDetailEditModel.ParentTaskId,
                        CreatedAt = dateTime,
                        CreatedBy = taskDetailEditModel.CreatedBy,
                        Status = taskDetailEditModel.Status.ToString().ToLower(),
                        Description = taskDetailEditModel.Description,
                        AssignedTo = taskDetailEditModel.AssignedTo,
                        Deadline = taskDetailEditModel.Deadline,
                        EditedAt = dateTime,
                        ExpectedHours = taskDetailEditModel.ExpectedHours,
                        IsRemoved = false,
                        AcceptanceCriteria = taskDetailEditModel.AcceptanceCriteria ?? 0,

                    };
                    db.TaskDetail.Add(task);
                    db.SaveChanges();
                }
            }

            // Update Task Hierarchy
           // TaskHierarchyManagementService.UpdateTaskHierarchy(task.TaskId);
            return GetTaskById(task.TaskId);
        }
        
        public  bool DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                var childrenTaskIds = db.TaskDetail
                    .Where(x => x.ParentTaskId == taskId)
                    .Select(y => y.TaskId)
                    .ToList();
                if (childrenTaskIds.Count > 0)
                {
                    return false;
                }

                // Get Selected TasK
                TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                // Get TaskDetail Positioned after selected taskDetail
                TaskDetail taskAfter = db.TaskDetail
                    .FirstOrDefault(x => x.PositionAfter == existingTask.TaskId);



                if (existingTask != null)
                {
                    if (taskAfter != null)
                    {
                        taskAfter.PositionAfter = existingTask.PositionAfter;
                    }

                    db.TaskDetail.Remove(existingTask);
                    db.SaveChanges();
                }

                return true;

            }
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

        public TaskDetailEditModel GetTaskById(string taskId)
        {
            using (var db = new ErpContext())
            {

                TaskDetail existingTask = db.TaskDetail
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
                    EditedAt = existingTask.EditedAt
                };

                return taskDetailEditModel;
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
    }


}