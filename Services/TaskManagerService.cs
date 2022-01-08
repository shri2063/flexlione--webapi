using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;


namespace m_sort_server.Services
{
    public class TaskManagerService
    {
        public static List<TaskSheetItemEditModel> GetTaskList(string taskId, string include)
        {
            List<TaskSheetItemEditModel> taskListEditView = new List<TaskSheetItemEditModel>();
            using (var db = new ErpContext())
            {
               
                if (include.Contains("children"))
                {
                    return GetChildTaskList(taskId);
                }

                throw new KeyNotFoundException("Error in finding required task list");
            }
        }
        public static TaskSheetItemEditModel CreateOrUpdateTask(TaskSheetItemEditModel taskSheetItemEditModel)
        {

            // Get Position After for above task (And revise position of other tasks)
            taskSheetItemEditModel.PositionAfter = UpdatePositionOfTasks(taskSheetItemEditModel);

            return CreateOrUpdateTaskInDb(taskSheetItemEditModel);

        }

        public static void DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                if ((GetTaskList(taskId, "child").Count > 0))
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

        private static List<TaskSheetItemEditModel> CreateReorderedList(List<TaskSheetItemEditModel> tasks)
        {

            List<TaskSheetItemEditModel> reorderedList = new List<TaskSheetItemEditModel>();


            int i = 0;
            TaskSheetItemEditModel currentTask = new TaskSheetItemEditModel();
            TaskSheetItemEditModel previousTask = new TaskSheetItemEditModel();

            while (i < tasks.Count)
            {
                if (i == 0)
                {
                    currentTask = GetTaskPositionAfter(tasks, null);

                }
                else
                {
                    currentTask = tasks
                        .Find(x =>
                            x.PositionAfter == previousTask.TaskId);
                }

                reorderedList.Add(currentTask);
                previousTask = currentTask;
                i = i + 1;
            }

            return reorderedList;

        }

        private static TaskSheetItemEditModel GetTaskPositionAfter(List<TaskSheetItemEditModel> tasks, string positionAfter)
        {
            return tasks
                .Find(x =>
                    x.PositionAfter == positionAfter);
        }

        private static string UpdatePositionOfTasks(TaskSheetItemEditModel newTaskItemEditModel)
        {
            List<TaskSheetItemEditModel> taskListEditModels = GetTaskList(newTaskItemEditModel.ParentTaskId, "child");

            TaskSheetItemEditModel existingTaskAtNewPosItemEditModel = taskListEditModels
                .Find(y => y.PositionAfter == newTaskItemEditModel.PositionAfter);

            
            
            // Case 1: Position After not mentioned
            if (newTaskItemEditModel.PositionAfter == null || newTaskItemEditModel.PositionAfter == "")
            {
                // Case 1.A : If it is the first element
                if (CreateReorderedList(taskListEditModels).Count == 0)
                {
                    return null;
                }

                // Case 1.B: If its not first element
                // Dependency
                // A. Is there any task positioned below desired task
                UpdatePositionOfTaskBelowGivenTask(newTaskItemEditModel);
                
                return CreateReorderedList(taskListEditModels)
                    .Last()
                    .TaskId;
            }
            // Case 2: Position after mentioned
            // Dependency
            // A. Is there any task positioned at new position
            // B. Is there any task positioned below desired task

            UpdatePositionOfTaskBelowGivenTask(newTaskItemEditModel);
            // Case A.1: No existing task positioned at new Position

            if (existingTaskAtNewPosItemEditModel == null)
            {
                return newTaskItemEditModel.PositionAfter;
            }
            else
            {
                // Case A.2: Some task is positioned
                UpdatePositionedAfterInDB(
                    existingTaskAtNewPosItemEditModel.TaskId, newTaskItemEditModel.TaskId);

                return newTaskItemEditModel.PositionAfter;
            }
        }


        private static string UpdatePositionOfTaskBelowGivenTask(TaskSheetItemEditModel taskSheetItemEditModel)
        {
            List<TaskSheetItemEditModel> taskListEditModels = GetTaskList(taskSheetItemEditModel.ParentTaskId, "child");

           
            TaskSheetItemEditModel taskPositionedBelowItemEditModel = taskListEditModels
                .Find(y => y.PositionAfter == taskSheetItemEditModel.TaskId);
            
            
            return (taskPositionedBelowItemEditModel == null) ? 
                "":UpdatePositionedAfterInDB(
                    taskPositionedBelowItemEditModel.TaskId,
                    GetTaskById(taskSheetItemEditModel.TaskId).PositionAfter);
        }
        private static string UpdatePositionedAfterInDB(string taskId, string positionAfter)
        {
           // Check if task Id and position after is not the same

           if (GetTaskById(taskId).PositionAfter == taskId)
           {
               throw new  KeyNotFoundException("Task cannot be positioned after itself");
           }
            
            using (var db = new ErpContext())
            {
               

                TaskSheet task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException("Task does not exist");
                }

                task.PositionAfter = positionAfter;
                db.SaveChanges();

                return task.PositionAfter;
            }
            
        }

        private static TaskSheetItemEditModel CreateOrUpdateTaskInDb(TaskSheetItemEditModel taskSheetItemEditModel)
        {
            // Check if task Id and Position After is not the same

            if (taskSheetItemEditModel.PositionAfter == taskSheetItemEditModel.TaskId)
            {
                throw new KeyNotFoundException("Task cannot be positioned after itself");
            }
            
            // Check if position after is actually under the same parent
            if (taskSheetItemEditModel.PositionAfter != null)
            {
                if (GetTaskById(taskSheetItemEditModel.PositionAfter).ParentTaskId != taskSheetItemEditModel.ParentTaskId)
                {
                    throw new KeyNotFoundException("Position after mentioned cannot be assigned");
                }   
            }


            TaskSheet task;
            using (var db = new ErpContext())
            {
                task = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskSheetItemEditModel.TaskId);


                if (task != null) // update
                {
                    task.ParentTaskId = taskSheetItemEditModel.ParentTaskId;
                    task.CreatedAt = DateTime.Now;
                    task.CreatedBy = taskSheetItemEditModel.CreatedBy;
                    task.OnHold = taskSheetItemEditModel.OnHold;
                    task.Description = taskSheetItemEditModel.Description;
                    task.PositionAfter = taskSheetItemEditModel.PositionAfter;

                    db.SaveChanges();
                }
                else
                {
                    task = new TaskSheet
                    {
                        TaskId = GetNextAvailableId(),
                        ParentTaskId = taskSheetItemEditModel.ParentTaskId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = taskSheetItemEditModel.CreatedBy,
                        OnHold = taskSheetItemEditModel.OnHold,
                        Description = taskSheetItemEditModel.Description,
                        PositionAfter = taskSheetItemEditModel.PositionAfter
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

        private static List<TaskSheetItemEditModel> GetChildTaskList(string  taskId)
        {

            List<TaskSheetItemEditModel> taskListEditModels = new List<TaskSheetItemEditModel>();
            using (var db = new ErpContext())
            {
                List<string> taskIdList = db.TaskTree
                    .Where(x => x.ParentTaskId == taskId)
                    .Select(t => t.TaskId)
                    .ToList();

                taskIdList.ForEach(
                    x => taskListEditModels.Add(
                        GetTaskById(x)));

                return CreateReorderedList(taskListEditModels);
                
            }
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
                    CreatedBy = existingTask.CreatedBy,
                    OnHold = existingTask.OnHold,
                    Description = existingTask.Description,
                    PositionAfter = existingTask.PositionAfter
                };

                return taskSheetItemEditModel;
            }

        }


    }
}