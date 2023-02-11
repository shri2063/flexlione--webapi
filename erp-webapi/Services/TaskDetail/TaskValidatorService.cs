using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TaskValidatorService : ITaskValidatorService
    {
       
        private readonly ITaskRepository _taskRepository;
       private readonly ITaskSummaryRelationRepository _taskSummaryRelationRepository;
        private readonly IProfileRepository _profileRepository;
       
        public TaskValidatorService(
          ITaskRepository taskRepository, 
           ITaskSummaryRelationRepository taskSummaryRelationRepository,
            IProfileRepository profileRepository
            )
        {
        
           
            _taskRepository = taskRepository;
            _taskSummaryRelationRepository = taskSummaryRelationRepository;
            _profileRepository = profileRepository;
          
        }
        public Boolean CheckValidAssigneeFields(TaskDetailEditModel taskForUpdate, TaskDetailEditModel existingTask,
            string loggedInId)
        {
            return ((taskForUpdate.Description != existingTask.Description) && (existingTask.AssignedTo != loggedInId));
        }


        public Boolean CheckIfDeadlineUpdatedByManager(TaskDetailEditModel taskForUpdate, string loggedInId)
        {
            TaskDetailEditModel existingTask = _taskRepository.GetTaskById(taskForUpdate.TaskId);

            List<string> managers = _profileRepository.GetManagerIds(taskForUpdate.CreatedBy);

            return (!managers.Contains(loggedInId) && taskForUpdate.Deadline != existingTask.Deadline);


        }

        public Boolean CheckUpdatedFields(TaskDetailEditModel taskForUpdate)
        {
            TaskDetailEditModel existingTask = _taskRepository.GetTaskById(taskForUpdate.TaskId);
            return (taskForUpdate.Description != existingTask.Description
                    || taskForUpdate.AssignedTo != existingTask.AssignedTo
                    || taskForUpdate.CreatedBy != existingTask.CreatedBy);

        }
        
        public Boolean GetPickedUpStatus(TaskDetailEditModel taskForUpdate)
        {
            // time spent criteria is how much times in minute will be qualifies to say task has been picked up
            decimal timeSpentCriteria = 2;
            decimal timeSpent = 0;
            Boolean taskRunning = false;

            // list of task Summaries of taskId
            var taskSummaries = _taskSummaryRelationRepository.GetTaskSummaryIdsForTaskId(taskForUpdate.TaskId);
            // checking each task summary
            foreach (var taskSummary in taskSummaries)
            {

                // checking stopped task summaries
                timeSpent += taskSummary.SystemHours;
                   
                // checking running task summaries 
                if (taskSummary.Action == "start")
                {
                    taskRunning = DateTime.Now > taskSummary.Stamp.AddMinutes(Convert.ToInt32(timeSpentCriteria));
                 
                }
            }

            // return true if more than 2 min has been spent on task 
            
            return (timeSpent > (timeSpentCriteria/60) || taskRunning);
        }
    }
}