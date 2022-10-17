using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.LinkedListModel;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace flexli_erp_webapi.Services
{
    public class TaskManagementService
    {

        private readonly ITagTaskListRepository _tagTaskListRepository;
        public TaskManagementService(ITagTaskListRepository tagTaskListRepository)
        {
            _tagTaskListRepository = tagTaskListRepository;
        }
        public static TaskDetailEditModel GetTaskById(string taskId, string include = null)
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
                taskDetail.Children =  GetRankedChildTaskList(taskId);
            }
            if (include.Contains("siblings"))
            { 
                taskDetail.Siblings =  GetRankedChildTaskList(taskDetail.ParentTaskId);
            }
            
            if (include.Contains("dependency"))
            {
                taskDetail.UpStreamDependencies = DependencyManagementService
                    .GetUpstreamDependenciesByTaskId(taskId,"taskDetail");
                taskDetail.DownStreamDependencies = DependencyManagementService
                    .GetDownstreamDependenciesByTaskId(taskId,"taskDetail");
            }

            return taskDetail;

        }

        public static List<string> GetTaskIdsForSprint(string sprintId)
        {
            List<string> taskIds = new List<string>();
            
            var sprint = SprintManagementService.GetSprintById(sprintId);
            if (sprint == null)
            {
                return taskIds;
            }
            
            var planningStage = new List<SStatus> { SStatus.Planning, SStatus.RequestForApproval };
            if (planningStage.Contains(sprint.Status))
            {
                
                    using (var db = new ErpContext())
                    {

                        taskIds = db.TaskDetail
                            .Where(x => x.SprintId == sprintId)
                            .Select(y => y.TaskId)
                            .ToList();
                    }

                    return taskIds;
                
            }

            using (var db = new ErpContext())
           {

               taskIds = db.SprintReport
                   .Where(x => x.SprintId == sprintId)
                   .Select(y => y.TaskId)
                   .Distinct()
                   .ToList();
           }
           
           return taskIds;
           
        }
        
       
        public  static TaskDetailEditModel CreateOrUpdateTask(TaskDetailEditModel taskDetailEditModel)
        {
            bool newTask ; 
            // Validation 1: Check if Position after is valid

                // 1.1 -  Position After taskDetail Id exist 
            if (!GetChildTaskIdList(taskDetailEditModel.ParentTaskId).Contains(taskDetailEditModel.PositionAfter))
            {
                if (!string.IsNullOrEmpty(taskDetailEditModel.PositionAfter))
                {
                    throw new KeyNotFoundException("Position after is invalid");
                }
              
            }
            // taskDetail is not positioned after itself
            if (taskDetailEditModel.TaskId == taskDetailEditModel.PositionAfter)
            {
                throw new KeyNotFoundException("TaskDetail cannot be positioned after itself");
            }
            // Check if its a new task

            var result = from s in GetTaskIdList()
                select s.TaskId;
            var temp = result.ToList();
            
            if (result.ToList().Contains(taskDetailEditModel.TaskId))
            {
                newTask = false;
            }
            else
            {
                newTask = true;
            }
            
            
            // ToDo: Validation 2: Check if parent taskDetail id is valid
            
            
          
            // All fields updated except rank
            TaskDetailEditModel updatedTaskDetail = CreateOrUpdateTaskInDb(taskDetailEditModel);
            
           // Check if Ordering of position has been changed
           // If No: Ignore
           // If Yes: Change ranking of the taskDetail
            // Update Ranks in db

           if (CheckIfPositionHasChanged(taskDetailEditModel,newTask))
           {
               // dummy rank updated
               UpdateRankInDb(updatedTaskDetail.TaskId,Int32.MaxValue);
               List<TaskDetailEditModel> reorderedList = ReorderTaskList(taskDetailEditModel);

               List<TaskDetailEditModel> rankedTask = UpdateRankOfReorderedList(reorderedList);

               List<TaskDetailEditModel> positionedTask = UpdatePositionOfRankedTask(rankedTask);
               
               positionedTask.ForEach(x => UpdateRankInDb(x.TaskId,x.Rank));
               positionedTask.ForEach(x => UpdatePositionInDb(x.TaskId,x.PositionAfter));
           }

           
          
           return GetTaskById(updatedTaskDetail.TaskId);
        }
        
        public static List<TaskShortDetailEditModel> GetTaskIdList(string parentTaskId = null)
        {
            using (var db = new ErpContext())
            {
                if (parentTaskId == null)
                {
                    return db.TaskDetail
                        .Select(t => new TaskShortDetailEditModel()
                        {
                            TaskId = t.TaskId,
                            Description = t.Description,
                            Status = (EStatus) Enum.Parse(typeof(EStatus), t.Status, true)
                        })
                        .ToList();
                }
                return db.TaskDetail
                    .Where(t => t.ParentTaskId == parentTaskId)
                    .Select(t => new TaskShortDetailEditModel()
                    {
                        TaskId = t.TaskId,
                        Description = t.Description
                    })
                    .ToList();
                
            }
        }

       
        public static void DeleteTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                if ((GetTaskById(taskId, "children").Children.Count > 0))
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
        public static void RemoveTask(string taskId)
        {
            using (var db = new ErpContext())
            {
                if ((GetTaskById(taskId, "children").Children.FindAll(
                    x => x.IsRemoved == false).Count > 0))
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

        public static TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                //[Check]: Task does not exist
                if (existingTask == null)
                    throw new KeyNotFoundException("TaskDetail does not exist");
                
                // [Check]: Task is not linked to some other sprint
                if (existingTask.SprintId != null)
                {
                    throw new ConstraintException("task already link to sprint" +  existingTask.SprintId);
                }

                Sprint sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                //[Check]: Sprint does not exist
                if (sprint == null)
                    throw new KeyNotFoundException("Sprint does not exist");

                // [Check]: Sprint is in planning stage
                if (sprint.Status != SStatus.Planning.ToString())
                {
                    throw new ConstraintException("cannot link task to sprint as sprint is not in planning stage");
                }

                existingTask.SprintId = sprintId;
                db.SaveChanges();

                return GetTaskById(existingTask.TaskId);
            }
            
        }
        
        public static TaskDetailEditModel RemoveTaskFromSprint(string taskId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail existingTask = db.TaskDetail
                    .Include(x => x.TaskSchedules)
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                // Case: TaskDetail does not exist
                if (existingTask == null)
                    throw new KeyNotFoundException("TaskDetail does not exist");
                
                

                Sprint sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == existingTask.SprintId);

               // Case : Task Not link to any sprint
                if (sprint == null)
                {
                    return GetTaskById(existingTask.TaskId);
                }
                // Case: Sprint is already approved
                if (sprint.Approved && !sprint.Closed)
                {
                    throw new ConstraintException("cannot delete the task as sprint is already approved");
                }
                
                existingTask.SprintId = null;
                db.SaveChanges();

                return GetTaskById(existingTask.TaskId);
            }
            
        }
       

        private static List<TaskDetailEditModel> ReorderTaskList(TaskDetailEditModel newTaskDetailItemEditModel)
        {
            LinkedChildTaskHead head = LinkedListService.CreateLinkedList(
                    GetTaskById(newTaskDetailItemEditModel.ParentTaskId, "children").Children);
            List<TaskDetailEditModel> reorderedList = new List<TaskDetailEditModel>();

            

            while (head.Pointer.TaskDetail.TaskId != null)
            {
                LinkedChildTask pointerNext = head.Pointer.Next;
                if (head.Pointer.TaskDetail.TaskId == newTaskDetailItemEditModel.PositionAfter)
                {
                    head.Pointer.Next = new LinkedChildTask()
                    {
                        TaskDetail = newTaskDetailItemEditModel,
                        Next = pointerNext
                    };

                }
               
                reorderedList.Add(head.Pointer.TaskDetail);
                head.Pointer = head.Pointer.Next;
                
            }
            // Remove Null list created at end
            //reorderedList.RemoveAt(reorderedList.Count - 1);
            // New taskDetail will be created (again) at last
            // Why? Since we atr assigning int.max value as it rank
            // If position_after  = null -> do nothing
            // Else remove it
            if (newTaskDetailItemEditModel.PositionAfter != null)
            {
                reorderedList.RemoveAt(reorderedList.Count - 1);
            }
          
            return reorderedList;
        }

        
        private static List<TaskDetailEditModel> UpdateRankOfReorderedList(List<TaskDetailEditModel> task)
        {
           List<TaskDetailEditModel> rankedTask = new List<TaskDetailEditModel>();

           int i = 1;
           while (task.Count > 0)
           {
               TaskDetailEditModel currentTaskDetail = task.First();
               currentTaskDetail.Rank = i;
               rankedTask.Add(currentTaskDetail);
               
               task.RemoveAt(0);
               i = i + 1;
           }

           return rankedTask;
        }


        
        private static void UpdateRankInDb(string taskId, int? rank)
        {
           
            using (var db = new ErpContext())
            {
                TaskDetail task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (task == null)
                {
                    return;
                }

                task.Rank = rank;
                db.SaveChanges();
   
            }
            
        }
        
        private static void UpdatePositionInDb(string taskId, string position)
        {
           
            using (var db = new ErpContext())
            {
                TaskDetail task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                if (task == null)
                {
                    return;
                }

                task.PositionAfter = position;
                db.SaveChanges();
   
            }
            
        }

        private static TaskDetailEditModel CreateOrUpdateTaskInDb(TaskDetailEditModel taskDetailEditModel)
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
            TaskHierarchyManagementService.UpdateTaskHierarchy(task.TaskId);
            return GetTaskById(task.TaskId);
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
        
       
        private static List<TaskDetailEditModel> GetRankedChildTaskList(string  taskId)
        {

            List<TaskDetailEditModel> taskListEditModels = new List<TaskDetailEditModel>();

            List<string> taskIdList = GetChildTaskIdList(taskId);

                taskIdList.ForEach(
                    x => taskListEditModels.Add(
                        GetTaskById(x)));

                taskListEditModels = taskListEditModels
                    .OrderBy(x => x.Rank)
                    .ToList();
                return taskListEditModels;
                
            
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

        private static bool CheckIfPositionHasChanged(TaskDetailEditModel taskDetail,bool newTask)
        {
            if (newTask)
            {
                return true;
            }
            
            if (taskDetail.PositionAfter != GetTaskById(taskDetail.TaskId).PositionAfter)
            {
                return true;
            }

            return false;
         }

        private static List<TaskDetailEditModel> UpdatePositionOfRankedTask(List<TaskDetailEditModel> rankedTask)
        {
            string previousTaskId = null;
            List<TaskDetailEditModel> positionedTask = new List<TaskDetailEditModel>();
            while (rankedTask.Count > 0)
            {
                TaskDetailEditModel taskDetail = rankedTask.First();
                taskDetail.PositionAfter = previousTaskId;
                positionedTask.Add(taskDetail);
                
                previousTaskId = taskDetail.TaskId;
                rankedTask.RemoveAt(0);
            }

            return positionedTask;
        }

        public static int CalculateTaskScore(string taskId, string sprintId)
        {

            TaskDetailEditModel task = GetTaskById(taskId);
            List<SprintReportEditModel>
                    sprintReportLineItems = SprintReportManagementService.GetSprintReportForSprint(sprintId)
                        .FindAll(x => x.TaskId == taskId);
                    
                int complete = 0;
                int completeEssential = 0;
                int essential = 0;
            
                    
                sprintReportLineItems.ForEach(sprintReportLineItem =>
                {
                    
                    if (sprintReportLineItem.Essential)
                        essential++;

                    if (sprintReportLineItem.Essential && sprintReportLineItem.Status == CStatus.Completed && sprintReportLineItem.Approved != SApproved.False)
                    {
                        if(sprintReportLineItem.ResultType==CResultType.Numeric && Convert.ToInt32(sprintReportLineItem.Result)>=sprintReportLineItem.WorstCase && Convert.ToInt32(sprintReportLineItem.Result) <= sprintReportLineItem.BestCase)
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.Boolean && sprintReportLineItem.Result == "true")
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.File && sprintReportLineItem.Result != null)
                        {
                            complete++;
                            completeEssential++;
                        }
                    }
                        
                    else if (sprintReportLineItem.Status == CStatus.Completed && sprintReportLineItem.Approved != SApproved.False)
                    {
                        if(sprintReportLineItem.ResultType==CResultType.Numeric && Convert.ToInt32(sprintReportLineItem.Result)>=sprintReportLineItem.WorstCase && Convert.ToInt32(sprintReportLineItem.Result) <= sprintReportLineItem.BestCase)
                        {
                            complete++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.Boolean && sprintReportLineItem.Result == "true")
                        {
                            complete++;
                        }

                        if (sprintReportLineItem.ResultType == CResultType.File && sprintReportLineItem.Result != null)
                        {
                            complete++;
                        }
                    }

                });
                    
                if (completeEssential < essential || complete < task.AcceptanceCriteria)
                   return 0;

                return Convert.ToInt32(task.ExpectedHours / 3);
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

        public static void PublishActualScoresForTask(string taskId, string sprintId)
        {
            TaskDetail task;
            using (var db = new ErpContext())
            {
                task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);
                
                List<CheckListItemEditModel>
                    checkListItems = CheckListManagementService.GetCheckList(task.TaskId, null, null);
                    
                int complete = 0;
                int completeEssential = 0;
                int essential = 0;
                    
                checkListItems.ForEach(checkListItem =>
                {
                    SprintReport sprintReport = db.SprintReport
                        .FirstOrDefault(x =>
                            x.SprintId == sprintId && x.TaskId == taskId &&
                            x.CheckListItemId == checkListItem.CheckListItemId);
                    
                    if (checkListItem.Essential)
                        essential++;

                    if (checkListItem.Essential && checkListItem.Status == CStatus.Completed && sprintReport.Approved==SApproved.True.ToString())
                    {
                        if(checkListItem.ResultType==CResultType.Numeric && Convert.ToInt32(checkListItem.Result)>=checkListItem.WorstCase && Convert.ToInt32(checkListItem.Result) <= checkListItem.BestCase)
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (checkListItem.ResultType == CResultType.Boolean && checkListItem.Result == "true")
                        {
                            complete++;
                            completeEssential++;
                        }

                        if (checkListItem.ResultType == CResultType.File && checkListItem.Result != null)
                        {
                            complete++;
                            completeEssential++;
                        }
                    }
                        
                    else if (!checkListItem.Essential && checkListItem.Status == CStatus.Completed && sprintReport.Approved==SApproved.True.ToString())
                    {
                        if(checkListItem.ResultType==CResultType.Numeric && Convert.ToInt32(checkListItem.Result)>=checkListItem.WorstCase && Convert.ToInt32(checkListItem.Result) <= checkListItem.BestCase)
                        {
                            complete++;
                        }

                        if (checkListItem.ResultType == CResultType.Boolean && checkListItem.Result == "true")
                        {
                            complete++;
                        }

                        if (checkListItem.ResultType == CResultType.File && checkListItem.Result != null)
                        {
                            complete++;
                        }
                    }

                });
                    
                if (completeEssential < essential || complete < task.AcceptanceCriteria)
                    task.Score = 0;

                else if (complete >= task.AcceptanceCriteria && completeEssential == essential)
                    task.Score = Convert.ToInt32(task.ExpectedHours / 3);

                db.SaveChanges();
            }
        }
    }
}