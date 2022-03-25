using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;


namespace m_sort_server.Services
{
    public class TaskManagerService
    {
        public static List<TaskEditModel> GetTaskList(string taskId, string include)
        {
                if (include.Contains("children"))
                {
                   return GetRankedChildTaskList(taskId);
                  
                }

                throw new KeyNotFoundException("Error in finding required task list");
            
        }
        public static TaskEditModel CreateOrUpdateTask(TaskEditModel taskEditModel)
        {
            bool newTask = false; 
            // Validation 1: Check if Position after is valid

                // 1.1 -  Position After task Id exist 
            if (!GetChildTaskIdList(taskEditModel.ParentTaskId).Contains(taskEditModel.PositionAfter))
            {
                if (!string.IsNullOrEmpty(taskEditModel.PositionAfter))
                {
                    throw new KeyNotFoundException("Position after is invalid");
                }
              
            }
            // task is not positioned after itself
            if (taskEditModel.TaskId == taskEditModel.PositionAfter)
            {
                throw new KeyNotFoundException("Task cannot be positioned after itself");
            }

            
            // ToDo: Validation 2: Check if parent task id is valid
            
            // Check if task id already exist or needs to be assigned new
            if (!GetChildTaskIdList(taskEditModel.ParentTaskId).Contains(taskEditModel.TaskId))
            {
                taskEditModel.TaskId = GetNextAvailableId();
                newTask = true;
            }
          
            // All fields updated except rank
            TaskEditModel updatedTask = CreateOrUpdateTaskInDb(taskEditModel);
            
           // Check if Ordering of position has been changed
           // If No: Ignore
           // If Yes: Change ranking of the task
            // Update Ranks in db

           if (CheckIfPositionHasChanged(taskEditModel,newTask))
           {
               // dummy rank updated
               UpdateRankInDb(updatedTask.TaskId,Int32.MaxValue);
               List<TaskEditModel> reorderedList = ReorderTaskList(taskEditModel);

               List<TaskEditModel> rankedTask = UpdateRankOfReorderedList(reorderedList);

               List<TaskEditModel> positionedTask = UpdatePositionOfRankedTask(rankedTask);
               
               positionedTask.ForEach(x => UpdateRankInDb(x.TaskId,x.Rank));
               positionedTask.ForEach(x => UpdatePositionInDb(x.TaskId,x.PositionAfter));
           }

           // Async run tag updates for the given task
           // All those tags that are contained in the task will get updated
           Task.Run(() => TagManagementService.UpdateTagsContainingTask(taskEditModel));
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

      
       

        private static List<TaskEditModel> ReorderTaskList(TaskEditModel newTaskItemEditModel)
        {
            LinkedChildTaskHead head = LinkedListService.CreateLinkedList(
                    GetTaskList(newTaskItemEditModel.ParentTaskId, "children"));
            List<TaskEditModel> reorderedList = new List<TaskEditModel>();

            

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

        
        private static List<TaskEditModel> UpdateRankOfReorderedList(List<TaskEditModel> task)
        {
           List<TaskEditModel> rankedTask = new List<TaskEditModel>();

           int i = 1;
           while (task.Count > 0)
           {
               TaskEditModel currentTask = task.First();
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

        private static TaskEditModel CreateOrUpdateTaskInDb(TaskEditModel taskEditModel)
        {
           TaskSheet task;
           if (taskEditModel.Deadline == null)
           {
               taskEditModel.Deadline = DateTime.MaxValue;
           }
           using (var db = new ErpContext())
            {
                task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskEditModel.TaskId);


                if (task != null) // update
                {
                    task.ParentTaskId = taskEditModel.ParentTaskId;
                    task.CreatedBy = taskEditModel.CreatedBy;
                    task.Status = taskEditModel.Status.ToString();
                    task.Description = taskEditModel.Description;
                    task.AssignedTo = taskEditModel.AssignedTo;
                    task.Deadline = taskEditModel.Deadline;
                    task.Score = taskEditModel.Score;

                    db.SaveChanges();
                }
                else
                {
                    task = new TaskSheet
                    {
                        TaskId = taskEditModel.TaskId,
                        ParentTaskId = taskEditModel.ParentTaskId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = taskEditModel.CreatedBy,
                        Status = taskEditModel.Status.ToString(),
                        Description = taskEditModel.Description,
                        AssignedTo = taskEditModel.AssignedTo,
                        Deadline = taskEditModel.Deadline,
                        Score = taskEditModel.Score,
                        
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

        public static List<string> GetChildTaskIdList(string parentTaskId)
        {
            using (var db = new ErpContext())
            {
                return db.TaskTree
                    .Where(x => x.ParentTaskId == parentTaskId)
                    .Select(t => t.TaskId)
                    .ToList();
            }
        }
        
        public static List<string> GetTaskIdList()
        {
            using (var db = new ErpContext())
            {
                return db.TaskTree
                    .Select(t => t.TaskId)
                    .ToList();
            }
        }
        private static List<TaskEditModel> GetRankedChildTaskList(string  taskId)
        {

            List<TaskEditModel> taskListEditModels = new List<TaskEditModel>();

            List<string> taskIdList = GetChildTaskIdList(taskId);

                taskIdList.ForEach(
                    x => taskListEditModels.Add(
                        GetTaskById(x)));

                taskListEditModels = taskListEditModels
                    .OrderBy(x => x.Rank)
                    .ToList();
                return taskListEditModels;
                
            
        }
        
        
        
        public static TaskEditModel GetTaskById(string taskId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSheet existingTask = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                // Case: Task does not exist
                if (existingTask == null)
                    return null;
                
                // Case: Status is not mentioned.
                // Its a check, ideally it should never be null
                // Make it Yet To Start
                if (existingTask.Status == null )
                {
                    existingTask.Status = EStatus.yetToStart.ToString();
                }
                
                TaskEditModel taskEditModel = new TaskEditModel()
                {
                    TaskId = existingTask.TaskId,
                    ParentTaskId = existingTask.ParentTaskId,
                    CreatedAt = existingTask.CreatedAt,
                    Deadline = existingTask.Deadline,
                    CreatedBy = existingTask.CreatedBy,
                    AssignedTo = existingTask.AssignedTo,
                    Score = existingTask.Score,
                    Status =  (EStatus) Enum.Parse(typeof(EStatus), existingTask.Status, true),
                    Description = existingTask.Description,
                    PositionAfter = existingTask.PositionAfter,
                    Rank = existingTask.Rank
                };

                return taskEditModel;
            }

        }

        private static bool CheckIfPositionHasChanged(TaskEditModel task,bool newTask)
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

        private static List<TaskEditModel> UpdatePositionOfRankedTask(List<TaskEditModel> rankedTask)
        {
            string previousTaskId = null;
            List<TaskEditModel> positionedTask = new List<TaskEditModel>();
            while (rankedTask.Count > 0)
            {
                TaskEditModel task = rankedTask.First();
                task.PositionAfter = previousTaskId;
                positionedTask.Add(task);
                
                previousTaskId = task.TaskId;
                rankedTask.RemoveAt(0);
            }

            return positionedTask;
        }


    }
}