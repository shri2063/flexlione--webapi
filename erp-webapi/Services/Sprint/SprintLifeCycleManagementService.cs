using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class SprintLifeCycleManagementService: ISprintLifeCycleManagementService
    {

        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly IEnumerable<IScoreAllocationPolicy> _calculateScoreForTaskPolicies;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ICheckListRelationRepository _checkListRelationRepository;
        private readonly IProfileRepository _profileRepository;
        public SprintLifeCycleManagementService(ISprintRepository sprintRepository, ITaskRelationRepository taskRelationRepository,
            IEnumerable<IScoreAllocationPolicy> calculateScoreForTaskPolicies, ICheckListRepository checkListRepository,
            ISprintReportRepository sprintReportRepository, IProfileRepository profileRepository,
            ICheckListRelationRepository checkListRelationRepository, ITaskRepository taskRepository)
        {
            _sprintRepository = sprintRepository;
            _taskRelationRepository = taskRelationRepository;
            _calculateScoreForTaskPolicies = calculateScoreForTaskPolicies;
            _checkListRepository = checkListRepository;
            _sprintReportRepository = sprintReportRepository;
            _checkListRelationRepository = checkListRelationRepository;
            _taskRepository = taskRepository;
            _profileRepository = profileRepository;
        }
        public SprintEditModel RequestForApproval(string sprintId, string userId)
        {
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId && x.Owner == userId);
                
                // [check] : if sprint or user not exist
                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint Id or User Id does not exist");
                }
                // [check] : if status is not planning
                if (sprint.Status != SStatus.Planning.ToString())
                {
                    throw new ConstraintException("status is not planning, hence request for approval can't be made");
                }
                // [check] : if status has no task
                if ( _taskRelationRepository.GetTaskIdsForSprint(sprintId).Count == 0)
                {
                    throw new ConstraintException("No task has been added to the sprint");
                }
                // [check] : total expected hours of sprint should not be more than 6 hours * working days between sprint
                if (TotalExpectedHours(sprintId) > 6*ValidSprintDays(sprintId))
                {
                    throw new ConstraintException("expected hours more then total sprint time");
                }
                sprint.Status = SStatus.RequestForApproval.ToString();
                db.SaveChanges();
            }

            return _sprintRepository.GetSprintById(sprintId);
        }
        public SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            SprintEditModel sprint;
            
            sprint = _sprintRepository.GetSprintById(sprintId);
               // [check] : validate manager
                if(!CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to approve the sprint");
                }

                // [check] : if status is valid
                if (sprint.Status != SStatus.RequestForApproval)
                {
                    throw new ConstraintException("Sprint not requested for approval hence can't be approved");
                }
                
                // Add entries in sprint report before changing status so that error is thrown before approved
                new SprintReportManagementService(_sprintRepository,_checkListRepository,_sprintReportRepository, 
                    _taskRelationRepository,_taskRepository, _checkListRelationRepository, _profileRepository).AddSprintReportLineItemsForSprint(sprint.SprintId);
                _sprintRepository.UpdateSprintStatus(sprintId, SStatus.Approved.ToString());
              
            return _sprintRepository.GetSprintById(sprintId);
        }

        public SprintEditModel RequestForClosure(string sprintId, string userId)
        {
            SprintEditModel sprint;
            
                sprint = _sprintRepository.GetSprintById(sprintId);

                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint Id or User Id does not exist");
                }

                if (sprint.Status != SStatus.Approved)
                {
                    throw new ConstraintException("status is not approved, hence request for closure can't be made");
                }

                _sprintRepository.UpdateSprintStatus(sprintId, SStatus.RequestForClosure.ToString());
                return _sprintRepository.GetSprintById(sprintId);
        }

        public SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            SprintEditModel sprint;
            
                sprint = _sprintRepository.GetSprintById(sprintId);
                if (sprint == null)
                {
                    throw new ConstraintException("Sprint Id does not exist" + sprintId);
                }
                sprint.Score = 0;
                
                if(!CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to close the sprint");
                }

                if (sprint.Status != SStatus.RequestForClosure)
                {
                    throw new ConstraintException("Sprint not requested for closure hence can't be closed");
                }

                var taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
                
               
                // Scoring according to scoring Policy
                if (sprint.ScorePolicy != null)
                {
                    sprint.Score = new ScoreAllocationPolicySelectorPolicy(_calculateScoreForTaskPolicies).CalculateSprintScore(sprintId,
                         sprint.ScorePolicy);
                    // sprint.Score = _calculateScoreForTask.calculateSprintScore(sprintId, (EScorePolicyType) Enum.Parse(typeof(EScorePolicyType), sprint.ScorePolicy,true));
                }

                _sprintRepository.AddOrUpdateSprint(sprint);
                _sprintRepository.UpdateSprintStatus(sprintId, SStatus.Closed.ToString());
                // [Action] - remove task link to sprint
                var removedTask = new List<string>();
         
                taskIds.ForEach(x =>
                    {
                        _taskRelationRepository.RemoveTaskFromSprint(x);
                        removedTask.Add(x);
                    });
                
               // Provisional Score Sprint Report
                SprintReportManagementService.UpdateProvisionalScoreInSprintReport(sprintId);
         
            return _sprintRepository.GetSprintById(sprintId);
        }
        
        public  SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            SprintEditModel sprint;

            sprint = _sprintRepository.GetSprintById(sprintId);
                if (sprint == null)
                {
                    throw new KeyNotFoundException("Sprint does not exist: " + sprintId);
                }
                sprint.Score = 0;
                
                if(!CheckManagerValidity(sprint.Owner,approverId))
                {
                    throw new ArgumentException("Approver id is not eligible to review the sprint");
                }

                if (sprint.Status != SStatus.Closed)
                {
                    throw new ConstraintException("Sprint cannot be reviewed as status is not closed");
                }
                var taskIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
                
                // [Action] - Update Actual task Score
                taskIds.ForEach(x =>
                {
                    // sprint.Score = sprint.Score + CalculateTaskScore(x,sprintId);
                    
                    sprint.Score = new ScoreAllocationPolicySelectorPolicy(_calculateScoreForTaskPolicies).CalculateSprintScore(sprintId,
                         sprint.ScorePolicy);

                });
                _sprintRepository.UpdateSprintStatus(sprintId, SStatus.Reviewed.ToString());
                
            return _sprintRepository.GetSprintById(sprintId);
        }
        
        private static int TotalExpectedHours(string sprintId)
        {
            // calculate total expected hours of all tasks in the sprint
            using (var db = new ErpContext())
            {
                List<Decimal?> expectedHours = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.ExpectedHours)
                    .ToList();

                return Convert.ToInt32(expectedHours.Sum());
            }
        }

        private static int ValidSprintDays(string sprintId)
        {
            // Calculate monday to friday working days in sprint
            Sprint sprint;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);

                DateTime to = sprint.ToDate;
                DateTime from = sprint.FromDate;
                
                if (to < from)
                    throw new ArgumentException("To cannot be smaller than from.", nameof(to));

                int n = 0;
                DateTime nextDate = from;
                while(nextDate <= to.Date)
                {
                    if (nextDate.DayOfWeek != DayOfWeek.Saturday && nextDate.DayOfWeek != DayOfWeek.Sunday)
                        n++;
                    nextDate = nextDate.AddDays(1);
                }

                return n;
            }
        }

       Boolean CheckManagerValidity(string user, string manager)
        {
           return _profileRepository.GetManagerIds(user).Contains(manager);
        }
    }
}