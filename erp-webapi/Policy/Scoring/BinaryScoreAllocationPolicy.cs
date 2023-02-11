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
    public class BinaryScoreAllocationPolicy : IScoreAllocationPolicy
    
    
    {   private readonly ITaskRepository _taskRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintUnplannedTaskRepository _sprintUnplannedTaskRepository;
        private ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private ITaskScheduleRelationRepository _taskScheduleRelationRepository;
        private IProfileRepository _profileRepository;

        public BinaryScoreAllocationPolicy(ITaskRepository taskRepository,
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
            
          // checking score policy
            if (sprint.ScorePolicy == EScorePolicyType.BinaryScoreAllocationPolicy)
            {
                decimal sprintHours = 0;
                decimal totalWorkHours = 0;
                decimal plannedHours = 0;
                decimal unPlannedHours = 0;
                
                // call the sprint tasks
                List<string> taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);

                // getting checklist for task

                foreach (var taskId in taskIds)
                {
                    var task = _taskRepository.GetTaskById(taskId);
                    
                    decimal taskActualHours = 0;
                    
                    int completeTotal = 0;
                    int completeEssential = 0;
                    int essential = 0;

                    List<CheckListItemEditModel> checkLists = _checkListRepository.GetCheckList(taskId, EAssignmentType.Task);
                    
                    foreach (var checkList in checkLists)
                    {
                        if (checkList.Essential) essential++;

                        if (checkList.Essential && checkList.Status == CStatus.Completed)
                        {
                            if (checkList.ResultType == CResultType.Numeric &&
                                Convert.ToInt32(checkList.Result) >= checkList.WorstCase &&
                                Convert.ToInt32(checkList.Result) <= checkList.BestCase)
                            {
                                completeTotal++;
                                completeEssential++;
                            }

                            if (checkList.ResultType == CResultType.Boolean && checkList.Result == "true")
                            {
                                completeTotal++;
                                completeEssential++;
                            }

                            if (checkList.ResultType == CResultType.File && checkList.Result != null)
                            {
                                completeTotal++;
                                completeEssential++;
                            }
                        }

                        else if (!checkList.Essential && checkList.Status == CStatus.Completed)
                        {
                            if (checkList.ResultType == CResultType.Numeric &&
                                Convert.ToInt32(checkList.Result) >= checkList.WorstCase &&
                                Convert.ToInt32(checkList.Result) <= checkList.BestCase)
                            {
                                completeTotal++;
                            }

                            if (checkList.ResultType == CResultType.Boolean && checkList.Result == "true")
                            {
                                completeTotal++;
                            }

                            if (checkList.ResultType == CResultType.File && checkList.Result != null)
                            {
                                completeTotal++;
                            }
                        }

                    }
     
                    // only score for completing essential & above acceptance criteria
                    
                    
                    if (completeEssential < essential || completeTotal < task.AcceptanceCriteria)
                    {
                        taskActualHours = 0;
                    }
                    else
                    {
                        taskActualHours = task.ExpectedHours ?? default(decimal) ;
                    }

                    plannedHours += taskActualHours;

                    sprintHours += task.ExpectedHours ?? default(decimal);
                }
                
                var unplannedTasks = new SprintUnplannedTaskManagementService(_sprintRepository,_sprintUnplannedTaskRepository,
                    _taskRepository,_taskRelationRepository,_taskHierarchyRelationRepository,_taskScheduleRelationRepository, _profileRepository).GetUnPlannedTasksForSprint(sprintId);
                
                foreach (var unplannedTask in unplannedTasks)
                {
                    unPlannedHours += _sprintUnplannedTaskRepository.GetUnplannedTaskScoreData(sprintId, unplannedTask.TaskId).ApprovedHours ?? default(decimal);
                }

                totalWorkHours = plannedHours + unPlannedHours;
                
                // scaling for 10
                sprintScore = Math.Round( ( 10 * totalWorkHours / sprintHours  ), 1);
            }
            
            return sprintScore;
        }

        public EScorePolicyType getType()
        {
            return EScorePolicyType.BinaryScoreAllocationPolicy;
        }
    }
}
    
    
