using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;

using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class SprintManagementService
    {
       
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly ISprintRelationRepository _sprintRelationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskSummaryRelationRepository _taskSummaryRelationRepository;
        private readonly ISprintLifeCycleManagementService _sprintLifeCycleManagementService;
        private readonly ISprintUnplannedTaskManagementService _sprintUnplannedTaskManagementService;
       
        public SprintManagementService ( ISprintRepository sprintRepository, 
            ITaskRelationRepository taskRelationRepository,
            ISprintRelationRepository sprintRelationRepository,
            ITaskRepository taskRepository,
            ITaskSummaryRelationRepository taskSummaryRelationRepository,
            ISprintLifeCycleManagementService sprintLifeCycleManagementService,
            ISprintUnplannedTaskManagementService sprintUnplannedTaskManagementService
        )

        {  _sprintRepository = sprintRepository;
            _taskRelationRepository = taskRelationRepository;
            _sprintRelationRepository = sprintRelationRepository;
            _taskRepository = taskRepository;
            _taskSummaryRelationRepository = taskSummaryRelationRepository;
            _sprintLifeCycleManagementService = sprintLifeCycleManagementService;
            _sprintUnplannedTaskManagementService = sprintUnplannedTaskManagementService;

        }



        public List<SprintEditModel> GetSprintsByProfileId(string profileId, List<String> include,
            int? pageIndex = null, int? pageSize = null)
        {
            var sprints = _sprintRelationRepository.GetSprintsForProfileId(profileId,  pageIndex, pageSize);
            
            sprints.ForEach(x =>
            {
                if (include.Contains("plannedTask"))
                {
                    x.PlannedTasks = GetPlannedTasksForSprint(x.SprintId);
                }
            
                if (include.Contains("unPlannedTask"))
                {
                    x.UnPlannedTasks = _sprintUnplannedTaskManagementService.GetUnPlannedTasksForSprint(x.SprintId);
                }
            });
            return sprints;
        }
        
       public  SprintEditModel GetSprintById(string sprintId, List<string> include = null)
        {
                  var sprint = _sprintRepository.GetSprintById(sprintId);
                  if (include == null)
                  {
                      return sprint;
                  }
                  sprint.PlannedTasks = new List<TaskDetailEditModel>();
                  if (include.Contains("plannedTask"))
                  {
                      sprint.PlannedTasks = GetPlannedTasksForSprint(sprintId);
                  }
            
                  if (include.Contains("unPlannedTask"))
                  {
                      sprint.UnPlannedTasks = _sprintUnplannedTaskManagementService.GetUnPlannedTasksForSprint(sprintId);
                  }

                  return sprint;
        }

       
        public  SprintEditModel AddOrUpdateSprint(SprintEditModel sprintEditModel) {
            // [Check]: Previous all sprints closed in case of new sprint
            if (_sprintRepository.GetSprintById(sprintEditModel.SprintId) == null)
            {
                List<SprintEditModel> sprints = _sprintRelationRepository.GetSprintsForProfileId(sprintEditModel.Owner);

                if (sprints.Count > 0)
                {
                    var openSprints = sprints.FindAll(x => (!x.Closed))
                        .Count;
                    if (openSprints > 0)
                    {
                        throw new ConstraintException("All Previous sprints need to be closed");
                    }
                }

            }
              
            
            
            // [Check]: Sprint is in planning state in case of already created sprint

            if (_sprintRepository.GetSprintById(sprintEditModel.SprintId) != null)
            {
              if(_sprintRepository.GetSprintById(sprintEditModel.SprintId).Status != SStatus.Planning)
                  
                throw new ConstraintException("Sprint  can be updated only in planning stage ");
            }
            
            return _sprintRepository.AddOrUpdateSprint(sprintEditModel);

        }


        


        
        
        public  List<TaskDetailEditModel> GetPlannedTasksForSprint(string sprintId)
        {
            List<TaskDetailEditModel> plannedTasks = new List<TaskDetailEditModel>();
            List<string> taskDetailIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
            if(taskDetailIds != null){ taskDetailIds.ForEach(x =>
                plannedTasks.Add(_taskRepository.GetTaskById(x))) ;}

            foreach (var plannedTask in plannedTasks)
            {
                decimal actualHrs = 0;
              
                List<TaskSummaryEditModel> taskSummaries =  _taskSummaryRelationRepository.GetTaskSummaryIdsForTaskId(plannedTask.TaskId) ;
                
              
                foreach (var taskSummery in taskSummaries)
                {
                    actualHrs += taskSummery.ActualHour;
                }

                plannedTask.ActualHours = actualHrs;
            }

            return plannedTasks;
        }

       
        public SprintUnplannedTaskScoreEditModel UpdateUnplannedTaskHoursScore(string sprintId, string taskId, int hours, string profileId, string include)
        {
            SprintUnplannedTaskScoreEditModel actualHoursUpdate = new SprintUnplannedTaskScoreEditModel();
            if (include == "request")
                
            {
                actualHoursUpdate = RequestHours(sprintId, taskId, hours, profileId);
            }

            if (include == "approve")
            {
                actualHoursUpdate = ApproveHours(sprintId, taskId, hours, profileId);
            }

            return actualHoursUpdate;
        }
        
        private static decimal GetNewSprintNo(SprintEditModel sprintEditModel)
        {
            using (var db = new ErpContext())
            {
                // Max sprint running for an owner if No sprint yet, default = 1
                var sprintNo = db.Sprint
                    .Where(x=>x.Owner==sprintEditModel.Owner)
                    .Select(x => Convert.ToDecimal(x.SprintNo))
                    .DefaultIfEmpty(1)
                    .Max();

                // sprint with sprintNo.
                var prevSprint = db.Sprint
                    .FirstOrDefault(x => Convert.ToDecimal(x.SprintNo) == sprintNo);
                
                // if prevSprint doesn't exist then return with 1
                if (prevSprint == null)
                {
                    return sprintNo;
                }

                // [Check]: Condition for main sprint
                // Assign +1 Number.
                if (sprintEditModel.FromDate > prevSprint.ToDate &&
                    sprintEditModel.ToDate.Subtract(sprintEditModel.FromDate).Days>=10)
                {
                    return Math.Truncate(sprintNo) + 1;
                }
                        
                // [Check]: Sprints with conditions other than main sprint are sub sprints
                // Assign +0.1 Number
                return sprintNo + Convert.ToDecimal(0.1);
            }
        }


        public SprintEditModel RequestForApproval(string sprintId, string userId)
        {
           return _sprintLifeCycleManagementService
                .RequestForApproval(sprintId, userId);
        }

        public SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
          return _sprintLifeCycleManagementService
                .ApproveSprint(sprintId, approverId);
        }

        public SprintEditModel RequestForClosure(string sprintId, string userId)
        {
            return _sprintLifeCycleManagementService
                .RequestForClosure(sprintId, userId);
        }

        public SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            return _sprintLifeCycleManagementService
                .CloseSprint(sprintId, approverId);
        }

        public SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            return _sprintLifeCycleManagementService
                .ReviewCompleted(sprintId, approverId);
        }

        public SprintUnplannedTaskScoreEditModel RequestHours(string sprintId, string taskId, int hours, string profileId)
        {
            return _sprintUnplannedTaskManagementService
                .RequestHours(
                    sprintId, taskId, hours, profileId);
        }

        public SprintUnplannedTaskScoreEditModel ApproveHours(string sprintId, string taskId, int hours, string profileId)
        {
            return _sprintUnplannedTaskManagementService
                .ApproveHours(
                    sprintId, taskId, hours, profileId);
        }
    }
}
