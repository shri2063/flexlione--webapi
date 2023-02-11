using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using flexli_erp_webapi.Shared;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TaskManagementService
    {
        private readonly ITagContext _tagContext;
        private readonly TaskSearchResultRelationRepository _taskSearchResultRelationRepository;
        private readonly ILabelRelationRepository _labelRelationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IDependencyRepository _dependencyRepository;
        private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private readonly ITaskRankingManagementService _taskRankingManagementService;
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskAnchorRepository _taskAnchorRepository;
        private readonly ITaskValidatorService _taskValidatorService;
        
        public TaskManagementService(
            ILabelRelationRepository labelRelationRepository, TaskSearchResultRelationRepository taskSearchResultRelationRepository,
             ITaskRepository taskRepository, IDependencyRepository dependencyRepository,
            ITaskHierarchyRelationRepository taskHierarchyRelationRepository, ITaskRankingManagementService taskRankingManagementService,
            ISprintRepository sprintRepository, ITaskAnchorRepository taskAnchorRepository, ITaskValidatorService taskValidatorService)
        {
            _labelRelationRepository = labelRelationRepository;
            _taskSearchResultRelationRepository = taskSearchResultRelationRepository;
            _taskRepository = taskRepository;
            _dependencyRepository = dependencyRepository;
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
            _taskRankingManagementService = taskRankingManagementService;
            _sprintRepository = sprintRepository;
            _taskAnchorRepository = taskAnchorRepository;
            _taskValidatorService = taskValidatorService;
        }

        public  TaskDetailEditModel GetTaskById(string taskId, string include = null)
        {
            TaskDetailEditModel taskDetail = _taskRepository.GetTaskById(taskId);

            if (taskDetail == null)
            {
                    throw new KeyNotFoundException("Error in finding required taskDetail list");
            }
            if (include == null)
            {
                // adding task labels
                taskDetail.Label = _taskAnchorRepository.GetLabel(taskId).Result;
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
        
        public async Task<TaskDetailEditModel> CreateOrUpdateTask(TaskDetailEditModel taskDetailEditModel, string loggedInId)
        {
            if (taskDetailEditModel.Deadline == null)
            {
                taskDetailEditModel.Deadline = DateTime.Today;
            }

            var existingTask = _taskRepository.GetTaskById(taskDetailEditModel.TaskId);

            if (existingTask != null)
            {
                // Assignee cannot be changed once Sprint is allocated
                if (existingTask.AssignedTo != taskDetailEditModel.AssignedTo 
                    && existingTask.SprintId != null)
                {
                    throw new Exception("Assignee cannot be changed. Already allocated in sprint");
                }
                
                // task field lock if spent more than 2 min
                if ( _taskValidatorService.GetPickedUpStatus(taskDetailEditModel) &&  _taskValidatorService.CheckUpdatedFields(taskDetailEditModel))
                {
                    throw new Exception("Assigned to, Created by & Task description can't be updated once the task has been picked up");
                }
                
                // deadline field can be updated by manager only
                if ( _taskValidatorService.CheckIfDeadlineUpdatedByManager(taskDetailEditModel, loggedInId))
                {
                    throw new Exception("Deadline can only be updated by manager");
                }
                
                // If other than assignee updating the description
                if ( _taskValidatorService.CheckValidAssigneeFields(taskDetailEditModel, existingTask, loggedInId))
                {
                    throw new Exception("only Assignee is allowed to update description & checklist");
                }
                
                //[Action] If Sprint status = planning or Sprint not allocated then you can change acceptance criteria
                if (existingTask.AcceptanceCriteria != taskDetailEditModel.AcceptanceCriteria)
                {
                    var sprint = _sprintRepository.GetSprintById(existingTask.SprintId);
                    var sprintInProgress = new[] { "RequestForApproval", "Approved", "RequestForClosure" };
                    
                    if (sprint != null ? sprintInProgress.Contains(sprint.Status.ToString()): false)
                    {
                        throw new Exception("Sprint Acceptance criteria cannot be modified once sprint is not in planning stage");
                    }
                }
            }
            
            // All fields updated except rank
            TaskDetailEditModel updatedTaskDetail =  _taskRepository.CreateOrUpdateTask(taskDetailEditModel);

            if (existingTask == null || existingTask.PositionAfter != updatedTaskDetail.PositionAfter)
            {
                // Updating mongo db in a separate thread (if some error comes it will die down silently)
                Task.Run(() =>  UpdateRankingOfTask(updatedTaskDetail));
            }

            if (existingTask == null || (existingTask.ParentTaskId != taskDetailEditModel.ParentTaskId))
            {
                _taskHierarchyRelationRepository.UpdateTaskHierarchy(updatedTaskDetail.TaskId);
            }

            if (existingTask != null && (existingTask.Description != taskDetailEditModel.Description))
            {
                await _taskSearchResultRelationRepository.RemoveFromSearchResults(updatedTaskDetail.TaskId);
            }
            
            if (existingTask == null || (existingTask.Description != taskDetailEditModel.Description))
            {
                // Updating mongo db in a separate thread (if some error comes it will die down silently)
                Task.Run(() =>  _taskSearchResultRelationRepository.AddToSearchResults(updatedTaskDetail));
            }
            else
            {
                _taskSearchResultRelationRepository.UpdateTaskSearchViews(updatedTaskDetail);
            }
            
            return  GetTaskById(updatedTaskDetail.TaskId);

        }
        

        public void DeleteTaskById(string taskId)
        {
            TaskDetailEditModel task = GetTaskById(taskId, "children");

            if (task.Children != null)
            {
                if (task.Children.Any(x => x.IsRemoved == false))
                {
                    throw new Exception("Task cannot be deleted. One or more child taks exisits");
                }
            }

            _taskHierarchyRelationRepository.DeleteTaskHierarchy(taskId);
            _taskRepository.DeleteTask(taskId);
            _taskRankingManagementService.RemoveRankingOfTask(task);
          
        }
        
        public void RemoveTaskById(string taskId)
        {
            TaskDetailEditModel task = GetTaskById(taskId, "children");

            if (task.Children != null)
            {
                if (task.Children.Any(x => x.IsRemoved == false))
                {
                    throw new Exception("Task cannot be removed. One or more child taks exisits");
                }
            }

            _taskRepository.RemoveTask(taskId);
            _taskRankingManagementService.RemoveRankingOfTask(task);
        }
        
        public async Task<TaskDetailEditModel> AddLabelToTask(string taskId, List<string> labels)

        {
            foreach (var label in labels)

                if (label == "sprint")
                {
                     await _labelRelationRepository.AddSprintLabelToTask(taskId);
                }

            // throw new ArgumentException("include has invalid label");}
            
            TaskDetailEditModel task = GetTaskById(taskId);

            task.Label = await _taskAnchorRepository.ReviseLabel(taskId, labels);
            
            return  task;

        }

        public Task<List<TaskDetailEditModel>> GetChildTaskRankingForTask(string parentTaskId)
            {
                return _taskRankingManagementService
                    .GetChildTaskRankingForTask(parentTaskId);
            }
        
        public Task<List<string>> UpdateRankingOfTask(TaskDetailEditModel task)
            {
                return _taskRankingManagementService
                    .UpdateRankingOfTask(task.Clone());
            }
    }
}