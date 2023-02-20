using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services.Scoring
{
    public class IncrementalScoreAllocationPolicy : IScoreAllocationPolicy
    {
        
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintUnplannedTaskRepository _sprintUnplannedTaskRepository;
        private ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private ITaskScheduleRelationRepository _taskScheduleRelationRepository;
        private IProfileRepository _profileRepository;
        public IncrementalScoreAllocationPolicy (ITaskRepository taskRepository,
            ITaskRelationRepository taskRelationRepository, 
            ISprintRepository sprintRepository,
            ICheckListRepository checkListRepository,
            ISprintUnplannedTaskRepository sprintUnplannedTaskRepository,
            ITaskHierarchyRelationRepository taskHierarchyRelationRepository,
            ITaskScheduleRelationRepository taskScheduleRelationRepository,
            IProfileRepository profileRepository)
        
        {
            _taskRelationRepository = taskRelationRepository;
            _sprintRepository = sprintRepository;
            _taskRepository = taskRepository;
            _checkListRepository = checkListRepository;
            _sprintUnplannedTaskRepository = sprintUnplannedTaskRepository;
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
            _taskScheduleRelationRepository = taskScheduleRelationRepository;
            _profileRepository = profileRepository;

        }
        
        
        public decimal calculateScore(string sprintId)
        {
            decimal sprintScore = 0;
            SprintEditModel sprint = _sprintRepository.GetSprintById(sprintId);
          
            if (sprint.ScorePolicy == EScorePolicyType.IncrementalScoreAllocationPolicy)
            {
                decimal totalWorkHours = 0;
                decimal plannedHours = 0;
                decimal unPlannedHours = 0;
                int sprintHours = 0;
                
                List<string> taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
                
                // for each task of sprint calculate the score
                foreach (var taskId in taskIds)
                {
                    decimal taskActualHours = 0;
                    int complete = 0;
                    
                    var task = _taskRepository.GetTaskById(taskId);
                  
                    List<CheckListItemEditModel> checkLists = _checkListRepository.GetCheckList(taskId, EAssignmentType.Task);
                    
                    foreach (var checkList in checkLists)
                    {
                        if (checkList.Status == CStatus.Completed)
                        {
                            complete++;
                        }
                    }

                    taskActualHours = Decimal.Divide( complete, checkLists.Count) * (task.ExpectedHours) ?? default(decimal);
                    sprintHours += Convert.ToInt32(task.ExpectedHours);
                    plannedHours += taskActualHours;
                }

                var unplannedTaskIds = new SprintUnplannedTaskManagementService(_sprintRepository,_sprintUnplannedTaskRepository,
                    _taskRepository,_taskRelationRepository,_taskHierarchyRelationRepository,_taskScheduleRelationRepository, _profileRepository).GetUnPlannedTasksForSprint(sprintId);
                
                
                foreach (var unplannedTask in unplannedTaskIds)
                {
                    unPlannedHours += _sprintUnplannedTaskRepository.GetUnplannedTaskScoreData(sprintId, unplannedTask.TaskId).ApprovedHours ?? default(decimal);
                }

                totalWorkHours = plannedHours + unPlannedHours;
                // if some one add a task with zero planned hrs [check]
                sprintScore = sprintHours == 0 ? 0 : Math.Round( ( 10 * totalWorkHours / sprintHours  ), 1);
            }

            return sprintScore;
        }

        public EScorePolicyType getType()
        {
            return EScorePolicyType.IncrementalScoreAllocationPolicy;
        }
    }
}