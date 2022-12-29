using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.LinkedListModel;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using flexli_erp_webapi.Shared;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;



namespace flexli_erp_webapi.Services
{
    public class TaskManagementService: ITaskRankingManagementService
    {
        
        private readonly AutoSearchByTagCompiler _autoSearchByTagCompiler;
        private readonly ILabelRelationRepository _labelRelationRepository;
        private readonly ITaskRankingRepository _taskRankingRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IDependencyRepository _dependencyRepository;
        private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;

        public TaskManagementService(
            ILabelRelationRepository labelRelationRepository, AutoSearchByTagCompiler autoSearchByTagCompiler,
            ITaskRankingRepository taskRankingRepository, ITaskRepository taskRepository, IDependencyRepository dependencyRepository,
            ITaskHierarchyRelationRepository taskHierarchyRelationRepository)
        {
        
            _labelRelationRepository = labelRelationRepository;
            _autoSearchByTagCompiler = autoSearchByTagCompiler;
            _taskRankingRepository = taskRankingRepository;
            _taskRepository = taskRepository;
            _dependencyRepository = dependencyRepository;
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;

        }

       

        public   TaskDetailEditModel GetTaskById(string taskId, string include = null)
        {
            TaskDetailEditModel taskDetail = GetTaskByIdFromDb(taskId);

            if (taskDetail == null)
            {
                    throw new KeyNotFoundException("Error in finding required taskDetail list");
            }

            if (include == null)
            {
                return taskDetail;
            }
            
            if (include.Contains("children"))
            {
                taskDetail.Children =  GetChildTaskRankingForTask(taskDetail.TaskId).Result;
            }
            if (include.Contains("siblings"))
            { 
                taskDetail.Siblings =    GetChildTaskRankingForTask(taskDetail.ParentTaskId).Result;
            }
            
            if (include.Contains("dependency"))
            {
                taskDetail.UpStreamDependencies = _dependencyRepository
                    .GetUpstreamDependenciesByTaskId(taskId);
                taskDetail.UpStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel =  _taskRepository.GetTaskById(x.DependentTaskId));
                taskDetail.DownStreamDependencies = _dependencyRepository
                    .GetDownstreamDependenciesByTaskId(taskId);
                taskDetail.DownStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel =  _taskRepository.GetTaskById(x.DependentTaskId));
            }

            return taskDetail;

        }

    
        
       
        public TaskDetailEditModel CreateOrUpdateTask(TaskDetailEditModel taskDetailEditModel)
        {

            // All fields updated except rank
            TaskDetailEditModel updatedTaskDetail =  CreateOrUpdateTaskInDb(taskDetailEditModel);
            
            _autoSearchByTagCompiler.AddToSearchResults(updatedTaskDetail.Description, updatedTaskDetail.TaskId, ECheckListType.Task);

            return  GetTaskById(updatedTaskDetail.TaskId);
        }
        
       

        


        public   void DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                var task =  GetTaskById(taskId, "children");
                if (( task.Children.Count > 0))
                {
                    throw new KeyNotFoundException("TaskDetail cannot be deleted. Contains one or more child taskDetail");
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


            }
        }
        
        // Removed task will not be shown in Web App until forced
        public  void RemoveTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                var task =  GetTaskById(taskId, "children");
                if (task.Children.FindAll(
                    x => x.IsRemoved == false).Count > 0)
                {
                    throw new KeyNotFoundException("Task cannot be removed. Contains one or more child taskDetail");
                }

                // Get Selected TasK
                TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
             
                if (existingTask != null)
                {
                    existingTask.IsRemoved = true;
                    db.SaveChanges();
                }


            }
        }

       
       

        

        
       
     
        private  TaskDetailEditModel CreateOrUpdateTaskInDb(TaskDetailEditModel taskDetailEditModel)
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
                   _autoSearchByTagCompiler.RemoveFromSearchResults(task.TaskId, ECheckListType.Task);
                    
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
                   if (task.SprintId != null? task.Status == SStatus.Planning.ToString(): true  )
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
           _taskHierarchyRelationRepository.UpdateTaskHierarchy(task.TaskId);
           

           var createdTask =  GetTaskById(task.TaskId);

           return createdTask;
        }
        
        
        private static string GetNextAvailableId()
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

        public static List<string> GetChildTaskIdList(string parentTaskId)
        {
            using (var db = new ErpContext())
            {
                return db.TaskDetail
                    .Where(x => x.ParentTaskId == parentTaskId)
                    .Select(t => t.TaskId)
                    .ToList();
            }
        }
        
       
        
        
        
        public static TaskDetailEditModel GetTaskByIdFromDb(string taskId)
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
                if (existingTask.Status == null )
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
                    Status =  (EStatus) Enum.Parse(typeof(EStatus), existingTask.Status, true),
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

     
       
        

        public static void UpdateEditedAt(string taskId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail taskDetail = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                taskDetail.EditedAt = DateTime.Now;
                db.SaveChanges();
            }
        }

       

        public async Task<SprintLabelTask> AddLabelToTask(string taskId, string label)
        {
            if (label == "sprint")
            {
                return await _labelRelationRepository.AddSprintLabelToTask(taskId);
            }

            throw new ArgumentException("include has invalid label");
        }

        public Task<List<TaskDetailEditModel>> GetChildTaskRankingForTask(string parentTaskId)
        {
            return new TaskRankingManagementService(_taskRankingRepository, _taskRepository)
                .GetChildTaskRankingForTask(parentTaskId);
        }

        public Task<List<string>> UpdateRankingOfTask(TaskDetailEditModel task)
        {
            return new TaskRankingManagementService(_taskRankingRepository, _taskRepository)
                .UpdateRankingOfTask(task.Clone());
        }
    }
}