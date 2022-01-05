using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;


namespace m_sort_server.Services
{
    public class TaskManagerService
    {
        public static List<TaskSheetEditModel> GetTaskList(string taskId, string include)
        {
            List<TaskSheetEditModel> taskListEditView = new List<TaskSheetEditModel>();
            using (var db = new ErpContext())
            {
               
                if (include.Contains("child"))
                {
                    return GetChildTaskList(taskId);
                }

                throw new KeyNotFoundException("Error in finding required task list");
            }
        }
        public static TaskSheetEditModel CreateOrUpdateTask(TaskSheetEditModel taskSheetEditModel)
        {

            // Get Position After for above task (And revise position of other tasks)
            taskSheetEditModel.PositionAfter = UpdatePositionOfTasks(taskSheetEditModel);

            return CreateOrUpdateTaskInDb(taskSheetEditModel);

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

        private static List<TaskSheetEditModel> CreateReorderedList(List<TaskSheetEditModel> tasks)
        {

            List<TaskSheetEditModel> reorderedList = new List<TaskSheetEditModel>();


            int i = 0;
            TaskSheetEditModel currentTask = new TaskSheetEditModel();
            TaskSheetEditModel previousTask = new TaskSheetEditModel();

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

        private static TaskSheetEditModel GetTaskPositionAfter(List<TaskSheetEditModel> tasks, string positionAfter)
        {
            return tasks
                .Find(x =>
                    x.PositionAfter == positionAfter);
        }

        private static string UpdatePositionOfTasks(TaskSheetEditModel newTaskEditModel)
        {
            List<TaskSheetEditModel> taskListEditModels = GetTaskList(newTaskEditModel.ParentTaskId, "child");

            TaskSheetEditModel existingTaskAtNewPosEditModel = taskListEditModels
                .Find(y => y.PositionAfter == newTaskEditModel.PositionAfter);

            
            
            // Case 1: Position After not mentioned
            if (newTaskEditModel.PositionAfter == null || newTaskEditModel.PositionAfter == "")
            {
                // Case 1.A : If it is the first element
                if (CreateReorderedList(taskListEditModels).Count == 0)
                {
                    return null;
                }

                // Case 1.B: If its not first element
                // Dependency
                // A. Is there any task positioned below desired task
                UpdatePositionOfTaskBelowGivenTask(newTaskEditModel);
                
                return CreateReorderedList(taskListEditModels)
                    .Last()
                    .TaskId;
            }
            // Case 2: Position after mentioned
            // Dependency
            // A. Is there any task positioned at new position
            // B. Is there any task positioned below desired task

            UpdatePositionOfTaskBelowGivenTask(newTaskEditModel);
            // Case A.1: No existing task positioned at new Position

            if (existingTaskAtNewPosEditModel == null)
            {
                return newTaskEditModel.PositionAfter;
            }
            else
            {
                // Case A.2: Some task is positioned
                UpdatePositionedAfterInDB(
                    existingTaskAtNewPosEditModel.TaskId, newTaskEditModel.TaskId);

                return newTaskEditModel.PositionAfter;
            }
        }


        private static string UpdatePositionOfTaskBelowGivenTask(TaskSheetEditModel taskSheetEditModel)
        {
            List<TaskSheetEditModel> taskListEditModels = GetTaskList(taskSheetEditModel.ParentTaskId, "child");

           
            TaskSheetEditModel taskPositionedBelowEditModel = taskListEditModels
                .Find(y => y.PositionAfter == taskSheetEditModel.TaskId);
            
            
            return (taskPositionedBelowEditModel == null) ? 
                "":UpdatePositionedAfterInDB(
                    taskPositionedBelowEditModel.TaskId,
                    GetTaskById(taskSheetEditModel.TaskId).PositionAfter);
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

        private static TaskSheetEditModel CreateOrUpdateTaskInDb(TaskSheetEditModel taskSheetEditModel)
        {
            // Check if task Id and Position After is not the same

            if (taskSheetEditModel.PositionAfter == taskSheetEditModel.TaskId)
            {
                throw new KeyNotFoundException("Task cannot be positioned after itself");
            }
            
            // Check if position after is actually under the same parent
            if (GetTaskById(taskSheetEditModel.PositionAfter).ParentTaskId != taskSheetEditModel.ParentTaskId)
            {
                throw new KeyNotFoundException("Position after mentioned cannot be assigned");
            }

            using (var db = new ErpContext())
            {
                TaskSheet existingTask = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskSheetEditModel.TaskId);


                if (existingTask != null) // update
                {
                    TaskSheetEditModel existingTaskView = GetTaskById(existingTask.TaskId);


                    existingTask.ParentTaskId = taskSheetEditModel.ParentTaskId;
                    existingTask.CreatedAt = DateTime.Now;
                    existingTask.CreatedBy = taskSheetEditModel.CreatedBy;
                    existingTask.OnHold = taskSheetEditModel.OnHold;
                    existingTask.Description = taskSheetEditModel.Description;
                    existingTask.PositionAfter = taskSheetEditModel.PositionAfter;

                    db.SaveChanges();
                }
                else
                {
                    TaskSheet newTask = new TaskSheet
                    {
                        TaskId = taskSheetEditModel.TaskId,
                        ParentTaskId = taskSheetEditModel.ParentTaskId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = taskSheetEditModel.CreatedBy,
                        OnHold = taskSheetEditModel.OnHold,
                        Description = taskSheetEditModel.Description,
                        PositionAfter = taskSheetEditModel.PositionAfter
                    };
                    db.TaskTree.Add(newTask);
                    db.SaveChanges();
                }
            }

            return GetTaskById(taskSheetEditModel.TaskId);
        }

        private static List<TaskSheetEditModel> GetChildTaskList(string  taskId)
        {

            List<TaskSheetEditModel> taskListEditModels = new List<TaskSheetEditModel>();
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
        
        private static TaskSheetEditModel GetTaskById(string taskId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSheet existingTask = db.TaskTree
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (existingTask == null)
                    return null;
                TaskSheetEditModel taskSheetEditModel = new TaskSheetEditModel()
                {
                    TaskId = existingTask.TaskId,
                    ParentTaskId = existingTask.ParentTaskId,
                    CreatedAt = existingTask.CreatedAt,
                    CreatedBy = existingTask.CreatedBy,
                    OnHold = existingTask.OnHold,
                    Description = existingTask.Description,
                    PositionAfter = existingTask.PositionAfter
                };

                return taskSheetEditModel;
            }

        }


    }
}