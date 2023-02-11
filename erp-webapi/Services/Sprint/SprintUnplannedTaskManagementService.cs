using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.Services.Interfaces;
using m_sort_server.Repository.Interfaces;

namespace m
{
    public class SprintUnplannedTaskManagementService :ISprintUnplannedTaskManagementService

    {
  
    private ISprintRepository _sprintRepository;
    private ISprintUnplannedTaskRepository _sprintUnplannedTaskRepository;
   private ITaskRepository _taskRepository;
   private ITaskRelationRepository _taskRelationRepository;
   private ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
   private ITaskScheduleRelationRepository _taskScheduleRelationRepository;
   private IProfileRepository _profileRepository;
    public SprintUnplannedTaskManagementService( ISprintRepository sprintRepository,
      ISprintUnplannedTaskRepository sprintUnplannedTaskRepository,
        ITaskRepository  taskRepository,
      ITaskRelationRepository taskRelationRepository,
      ITaskHierarchyRelationRepository taskHierarchyRelationRepository,
      ITaskScheduleRelationRepository taskScheduleRelationRepository,
      IProfileRepository profileRepository
     )
    {
        _sprintRepository = sprintRepository;
        _sprintUnplannedTaskRepository = sprintUnplannedTaskRepository;
      _taskRepository = taskRepository;
      _taskRelationRepository = taskRelationRepository;
      _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
      _taskScheduleRelationRepository = taskScheduleRelationRepository;
      _profileRepository = profileRepository;
    }

    public SprintUnplannedTaskDataEditModel RequestHours(string sprintId, string taskId, int hours, string profileId)
    {

        // check if sprint owner indeed is sending request

        if ( _sprintRepository.GetSprintById(sprintId).Owner != profileId)
        {
            throw new ArgumentException("Id is not eligible to request for score");
        }

        // check if sprint is not reviewed yet
        if (_sprintRepository.GetSprintById(sprintId).Status == SStatus.Reviewed)
        {
            throw new ConstraintException("Can't update because sprint review is completed");
        }
        
        
        // check if task belong to sprint
        if (GetUnPlannedTasksForSprint(sprintId).FirstOrDefault(x => x.TaskId == taskId) == null)
        {
            throw new ArgumentException("Task does not belong to Sprint");
        }
        
        //check if sending request first time
        SprintUnplannedTaskDataEditModel unplannedDb;
        unplannedDb = _sprintUnplannedTaskRepository.GetUnplannedTaskScoreData(sprintId, taskId);
            
        if (unplannedDb != null && unplannedDb.ScoreStatus == EUnplannedTaskStatus.requested)
        {
            throw new ArgumentException("Task hours already requested");
        }

        
        // var db = new ErpContext();
        var newUnplanned = new SprintUnplannedTaskDataEditModel()
        {
            SprintId = sprintId,
            TaskId = taskId,
            RequestedHours = hours,
            ApprovedHours = 0,
            ScoreStatus = EUnplannedTaskStatus.requested,
            ProfileId = profileId
        };
        
        // updating task score & hrs
        TaskDetailEditModel unplannedTask = _taskRepository.GetTaskById(taskId);
        if (unplannedTask != null)
        {
            unplannedTask.Score = 0;
            unplannedTask.ActualHours = hours;
        }
        
        _taskRepository.CreateOrUpdateTask(unplannedTask);
        
        // updating sprint unplanned table
        return _sprintUnplannedTaskRepository.CreateOrUpdateSprintUnplannedTask(newUnplanned);
        
    }

    public SprintUnplannedTaskDataEditModel ApproveHours(string sprintId, string taskId, int hours, string profileId)
    {

        // check if manager is sending request
        List<string> managers =  _profileRepository.GetManagerIds(_sprintRepository.GetSprintById(sprintId).Owner);
        
        if (!managers.Contains(profileId))
        {
            throw new ArgumentException("Id is not eligible to approve hours");
        }

        // check if sprint is not reviewed yet
        if (_sprintRepository.GetSprintById(sprintId).Status== SStatus.Reviewed)
        {
            throw new ConstraintException("Can't update after sprint review");
        }

        // check if task belong to sprint
        if (GetUnPlannedTasksForSprint(sprintId).FirstOrDefault(x => x.TaskId == taskId) == null)
        {
            throw new ArgumentException("Task does not belong to Sprint");
        }

        //check if score is requested

       
        
            SprintUnplannedTaskDataEditModel unplannedDb;
            unplannedDb = _sprintUnplannedTaskRepository.GetUnplannedTaskScoreData(sprintId, taskId);
            
            if (unplannedDb == null)
            {
                throw new ArgumentException("Task for sprint does not exist");
            }

            if (unplannedDb.ScoreStatus != EUnplannedTaskStatus.requested)
            {
                throw new ArgumentException("Task score is reviewed or not requested");
            }

            unplannedDb.ScoreStatus = EUnplannedTaskStatus.reviewed;
            unplannedDb.ApprovedHours = hours;
            unplannedDb.ProfileId = profileId;
            
            // updating task score & hrs
            
            TaskDetailEditModel unplannedTask = _taskRepository.GetTaskById(taskId);
            if (unplannedTask != null)
            { 
                unplannedTask.Score = Math.Round(Decimal.Divide(hours, 3), 1); 
                unplannedTask.ActualHours = hours;
            }
            
            _taskRepository.CreateOrUpdateTask(unplannedTask);
            
            // updating sprint unplanned table
            return _sprintUnplannedTaskRepository.CreateOrUpdateSprintUnplannedTask(unplannedDb);
        

    }
    
    public  List<TaskDetailEditModel> GetUnPlannedTasksForSprint(string sprintId) {
            List<TaskDetailEditModel> unPlannedTasks = new List<TaskDetailEditModel>();
            List<string> plannedTaskDetailIds = _taskRelationRepository.GetTaskIdsForSprint(sprintId);
            var sprint = _sprintRepository.GetSprintById(sprintId);
            if (sprint == null)
            {
                return unPlannedTasks;
                
            }

            // WaterFall: get All Task Ids covered in Sprint Span
            var calenderTaskIds = (
                from s in 
                _taskScheduleRelationRepository.GetAllTaskScheduleByProfileIdAndDateRange(sprint.Owner, sprint.FromDate,
                sprint.ToDate)
                select s.TaskId)
                .Distinct()
                .ToList();
            
            var unPlannedTaskIds = new List<string>(calenderTaskIds);
            // Filter Task Ids  which have upstream in planned task ids
            foreach (var taskId in calenderTaskIds)
            {
                var upStreamTaskIds = (from s in
                        _taskHierarchyRelationRepository.GetTaskHierarchyByTaskIdFromDb(taskId).ChildrenTaskIdList
                    select s).ToList();
                
                var commonTaskIds = upStreamTaskIds.Intersect(plannedTaskDetailIds).ToList();

                if (commonTaskIds.Any())
                {
                     unPlannedTaskIds.Remove(taskId);
                }
            }
            
            // Filter Task Schedules which have upstream in another unplanned task schedules
            var filteredUnPlannedTaskIds = new List<string>(unPlannedTaskIds);
            
            foreach (var taskId in unPlannedTaskIds)
            {
                var upStreamTaskIds = (from s in
                        _taskHierarchyRelationRepository.GetTaskHierarchyByTaskIdFromDb(taskId).ChildrenTaskIdList
                    select s).ToList();
                upStreamTaskIds.RemoveAt(0);
                var commonTaskIds = upStreamTaskIds.Intersect(unPlannedTaskIds);

                if (commonTaskIds.Any())
                {
                    filteredUnPlannedTaskIds.Remove(taskId);
                }
            }
            
            
            unPlannedTaskIds.ForEach(x => unPlannedTasks.Add(_taskRepository.GetTaskById(x)));

            // adding unplanned task data to task 
            foreach (var unplannedTask in unPlannedTasks)
            {
                unplannedTask.UnplannedTaskData =
                    _sprintUnplannedTaskRepository.GetUnplannedTaskScoreData(sprintId, unplannedTask.TaskId);
            }
            return unPlannedTasks;
        }



    }
}
