using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;


namespace m_sort_server.Services
{
    public class TaskManagerService
    {
        public static List<TaskSheetItemEditModel> GetTaskList(string taskId, string include)
        {
                if (include.Contains("children"))
                {
                   return GetRankedChildTaskList(taskId);
                  
                }

                throw new KeyNotFoundException("Error in finding required task list");
            
        }
        public static TaskSheetItemEditModel CreateOrUpdateTask(TaskSheetItemEditModel taskSheetItemEditModel)
        {
            bool newTask = false; 
            // Validation 1: Check if Position after is valid

                // 1.1 -  Position After task Id exist 
            if (!GetChildTaskIdList(taskSheetItemEditModel.ParentTaskId).Contains(taskSheetItemEditModel.PositionAfter))
            {
                if (!string.IsNullOrEmpty(taskSheetItemEditModel.PositionAfter))
                {
                    throw new KeyNotFoundException("Position after is invalid");
                }
              
            }
            // task is not positioned after itself
            if (taskSheetItemEditModel.TaskId == taskSheetItemEditModel.PositionAfter)
            {
                throw new KeyNotFoundException("Task cannot be positioned after itself");
            }

            
            // ToDo: Validation 2: Check if parent task id is valid
            
            // Check if task id already exist or needs to be assigned new
            if (!GetChildTaskIdList(taskSheetItemEditModel.ParentTaskId).Contains(taskSheetItemEditModel.TaskId))
            {
                taskSheetItemEditModel.TaskId = GetNextAvailableId();
                newTask = true;
            }
          
            // All fields updated except rank
            TaskSheetItemEditModel updatedTask = CreateOrUpdateTaskInDb(taskSheetItemEditModel);
            
           // Check if Ordering of position has been changed
           // If No: Ignore
           // If Yes: Change ranking of the task
            // Update Ranks in db

           if (CheckIfPositionHasChanged(taskSheetItemEditModel,newTask))
           {
               // dummy rank updated
               UpdateRankInDb(updatedTask.TaskId,Int32.MaxValue);
               List<TaskSheetItemEditModel> reorderedList = ReorderTaskList(taskSheetItemEditModel);

               List<TaskSheetItemEditModel> rankedTask = UpdateRankOfReorderedList(reorderedList);

               List<TaskSheetItemEditModel> positionedTask = UpdatePositionOfRankedTask(rankedTask);
               
               positionedTask.ForEach(x => UpdateRankInDb(x.TaskId,x.Rank));
               positionedTask.ForEach(x => UpdatePositionInDb(x.TaskId,x.PositionAfter));
           }
            return GetTaskById(updatedTask.TaskId);
        }

        public static void DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                if ((GetTaskList(taskId, "children").Count > 0))
                {
                    throw new KeyNotFoundException("Task cannot be deleted. Contains one or more child task");
                }

                // Get Selected TasK
                TaskSheet existingTask = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                // Get Task Positioned after selected task
                TaskSheet taskAfter = db.TaskTree
                    .FirstOrDefault(x => x.PositionAfter == existingTask.TaskId);



                if (existingTask != null)
                {
                    if (taskAfter != null)
                    {
                        taskAfter.PositionAfter = existingTask.PositionAfter;
                    }

                    db.TaskTree.Remove(existingTask);
                    db.SaveChanges();
                }


            }
        }

      
       

        private static List<TaskSheetItemEditModel> ReorderTaskList(TaskSheetItemEditModel newTaskItemEditModel)
        {
            LinkedChildTaskHead head = LinkedListService.CreateLinkedList(
                    GetTaskList(newTaskItemEditModel.ParentTaskId, "children"));
            List<TaskSheetItemEditModel> reorderedList = new List<TaskSheetItemEditModel>();

            

            while (head.Pointer.Task.TaskId != null)
            {
                LinkedChildTask pointerNext = head.Pointer.Next;
                if (head.Pointer.Task.TaskId == newTaskItemEditModel.PositionAfter)
                {
                    head.Pointer.Next = new LinkedChildTask()
                    {
                        Task = newTaskItemEditModel,
                        Next = pointerNext
                    };

                }
               
                reorderedList.Add(head.Pointer.Task);
                head.Pointer = head.Pointer.Next;
                
            }
            // Remove Null list created at end
            //reorderedList.RemoveAt(reorderedList.Count - 1);
            // New task will be created (again) at last
            // Why? Since we atr assigning int.max value as it rank
            // If position_after  = null -> do nothing
            // Else remove it
            if (newTaskItemEditModel.PositionAfter != null)
            {
                reorderedList.RemoveAt(reorderedList.Count - 1);
            }
          
            return reorderedList;
        }

        
        private static List<TaskSheetItemEditModel> UpdateRankOfReorderedList(List<TaskSheetItemEditModel> task)
        {
           List<TaskSheetItemEditModel> rankedTask = new List<TaskSheetItemEditModel>();

           int i = 1;
           while (task.Count > 0)
           {
               TaskSheetItemEditModel currentTask = task.First();
               currentTask.Rank = i;
               rankedTask.Add(currentTask);
               
               task.RemoveAt(0);
               i = i + 1;
           }

           return rankedTask;
        }


        
        private static void UpdateRankInDb(string taskId, int? rank)
        {
           
            using (var db = new ErpContext())
            {
                TaskSheet task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (task == null)
                {
                    return;
                }

                task.Rank = rank;
                db.SaveChanges();
   
            }
            
        }
        
        private static void UpdatePositionInDb(string taskId, string position)
        {
           
            using (var db = new ErpContext())
            {
                TaskSheet task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (task == null)
                {
                    return;
                }

                task.PositionAfter = position;
                db.SaveChanges();
   
            }
            
        }

        private static TaskSheetItemEditModel CreateOrUpdateTaskInDb(TaskSheetItemEditModel taskSheetItemEditModel)
        {
           TaskSheet task;
            using (var db = new ErpContext())
            {
                task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskSheetItemEditModel.TaskId);


                if (task != null) // update
                {
                    task.ParentTaskId = taskSheetItemEditModel.ParentTaskId;
                    task.CreatedBy = taskSheetItemEditModel.CreatedBy;
                    task.Status = taskSheetItemEditModel.Status;
                    task.Description = taskSheetItemEditModel.Description;
                    task.AssignedTo = taskSheetItemEditModel.AssignedTo;
                    task.Deadline = taskSheetItemEditModel.Deadline;
                    task.Score = taskSheetItemEditModel.Score;

                    db.SaveChanges();
                }
                else
                {
                    task = new TaskSheet
                    {
                        TaskId = taskSheetItemEditModel.TaskId,
                        ParentTaskId = taskSheetItemEditModel.ParentTaskId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = taskSheetItemEditModel.CreatedBy,
                        Status = taskSheetItemEditModel.Status,
                        Description = taskSheetItemEditModel.Description,
                        AssignedTo = taskSheetItemEditModel.AssignedTo,
                        Deadline = taskSheetItemEditModel.Deadline,
                        Score = taskSheetItemEditModel.Score,
                        
                    };
                    db.TaskTree.Add(task);
                    db.SaveChanges();
                }
            }

            return GetTaskById(task.TaskId);
        }
        
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.TaskTree
                    .Select(x => Convert.ToInt32(x.TaskId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }

        private static List<string> GetChildTaskIdList(string parentTaskId)
        {
            using (var db = new ErpContext())
            {
                return db.TaskTree
                    .Where(x => x.ParentTaskId == parentTaskId)
                    .Select(t => t.TaskId)
                    .ToList();
            }
        }
        private static List<TaskSheetItemEditModel> GetRankedChildTaskList(string  taskId)
        {

            List<TaskSheetItemEditModel> taskListEditModels = new List<TaskSheetItemEditModel>();

            List<string> taskIdList = GetChildTaskIdList(taskId);

                taskIdList.ForEach(
                    x => taskListEditModels.Add(
                        GetTaskById(x)));

                taskListEditModels = taskListEditModels
                    .OrderBy(x => x.Rank)
                    .ToList();
                return taskListEditModels;
                
            
        }
        
        
        
        private static TaskSheetItemEditModel GetTaskById(string taskId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSheet existingTask = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (existingTask == null)
                    return null;
                TaskSheetItemEditModel taskSheetItemEditModel = new TaskSheetItemEditModel()
                {
                    TaskId = existingTask.TaskId,
                    ParentTaskId = existingTask.ParentTaskId,
                    CreatedAt = existingTask.CreatedAt,
                    Deadline = existingTask.Deadline,
                    CreatedBy = existingTask.CreatedBy,
                    AssignedTo = existingTask.AssignedTo,
                    Score = existingTask.Score,
                    Status = existingTask.Status,
                    Description = existingTask.Description,
                    PositionAfter = existingTask.PositionAfter,
                    Rank = existingTask.Rank
                };

                return taskSheetItemEditModel;
            }

        }

        private static bool CheckIfPositionHasChanged(TaskSheetItemEditModel task,bool newTask)
        {
            if (newTask)
            {
                return true;
            }
            
            if (task.PositionAfter != GetTaskById(task.TaskId).PositionAfter)
            {
                return true;
            }

            return false;
         }

        private static List<TaskSheetItemEditModel> UpdatePositionOfRankedTask(List<TaskSheetItemEditModel> rankedTask)
        {
            string previousTaskId = null;
            List<TaskSheetItemEditModel> positionedTask = new List<TaskSheetItemEditModel>();
            while (rankedTask.Count > 0)
            {
                TaskSheetItemEditModel task = rankedTask.First();
                task.PositionAfter = previousTaskId;
                positionedTask.Add(task);
                
                previousTaskId = task.TaskId;
                rankedTask.RemoveAt(0);
            }

            return positionedTask;
        }


    }
}