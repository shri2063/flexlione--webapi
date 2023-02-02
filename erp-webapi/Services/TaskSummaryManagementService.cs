using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using m_sort_server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Services
{
    public class TaskSummaryManagementService : ITaskHourCalculatorHandler


    {

    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
    private readonly ITaskSummaryRepository _taskSummaryRepository;

    public TaskSummaryManagementService(ITaskRepository taskRepository,
        ITaskHierarchyRelationRepository taskHierarchyRelationRepository, ITaskSummaryRepository taskSummaryRepository)
    {
        _taskRepository = taskRepository;
        _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
        _taskSummaryRepository = taskSummaryRepository;
    }


    public List<TaskSummaryEditModel> GetDailyTaskSummary(string profileId, DateTime date, int? pageIndex = null,
        int? pageSize = null)
    {
        List<string> taskSummaryIds;
        List<TaskSummaryEditModel> taskSummaryList
            = new List<TaskSummaryEditModel>();
        using (var db = new ErpContext())
        {
            if (pageIndex != null && pageSize != null)
            {
                taskSummaryIds = GetTaskSummaryIdsPageForAProfileId(profileId, date, (int) pageIndex, (int) pageSize);
            }
            else
            {
                taskSummaryIds = db.TaskSummary
                    .Where(x => x.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                    .Include(x => x.TaskId)
                    .Where(x => x.TaskDetail.AssignedTo == profileId)
                    .Select(x => x.TaskSummaryId)
                    .ToList();
            }
        }

        taskSummaryIds.ForEach(x => taskSummaryList.Add(
            _taskSummaryRepository.GetTaskSummaryById(x)));
        taskSummaryList.ForEach(x =>
        {
            TaskDetailEditModel task = _taskRepository.GetTaskById(x.TaskId);
            x.Task = new TaskShortDetailEditModel()
            {
                TaskId = task.TaskId,
                Description = task.Description,
                Status = task.Status
            };
        });

        // short task schedule in task summary
        taskSummaryList.ForEach(x =>
        {
            x.TaskSchedule = TaskScheduleManagementService.GetShortTaskScheduleById(x.TaskScheduleId);
        });

        return taskSummaryList;

    }

    public List<TaskSummaryEditModel> GetAllTaskSummaryByTaskId(string taskId, DateTime? fromDate = null,
        DateTime? toDate = null, string include = null, int? pageIndex = null, int? pageSize = null)
    {
        List<string> taskSummaryIds;

        if (pageIndex != null && pageSize != null)
        {
            taskSummaryIds = GetTaskSummaryIdsPageForTaskId(taskId, (int) pageIndex, (int) pageSize);
        }
        else
        {
            taskSummaryIds = GetTaskSummaryIdsForTaskId(taskId);
        }

        List<TaskSummaryEditModel> taskSummaryList = new List<TaskSummaryEditModel>();
        taskSummaryList = GetFilteredTaskSummaryById(taskSummaryIds, fromDate, toDate);

        if (include == null)
        {
            return taskSummaryList;
        }

        List<TaskSummaryEditModel> childTaskSummaryList = new List<TaskSummaryEditModel>();
        if (include.Contains("allChildren"))
        {
            List<string>
                childTaskIds =
                    _taskHierarchyRelationRepository.GetDownStreamTaskIdsForTaskId(taskId); // includes parent taskId
            childTaskIds.Remove(taskId);
            List<string> childTaskSummaryIds = new List<string>();
            childTaskIds.ForEach(x => childTaskSummaryIds
                .AddRange(GetTaskSummaryIdsForTaskId(x)));

            childTaskSummaryList = GetFilteredTaskSummaryById(childTaskSummaryIds, fromDate, toDate);

        }

        return taskSummaryList.Concat(childTaskSummaryList).ToList();

    }

    public List<string> GetTaskSummaryIdsForTaskId(string taskId, string include = null)
    {
        List<string> taskSummaryIds;
        using (var db = new ErpContext())
        {
            taskSummaryIds = db.TaskSummary
                .Where(x => x.TaskId == taskId)
                .Select(x => x.TaskSummaryId)
                .ToList();
        }

        return taskSummaryIds;

    }

    private List<string> GetTaskSummaryIdsPageForTaskId(string taskId, int pageIndex, int pageSize)
    {
        using (var db = new ErpContext())
        {
            if (pageIndex <= 0 || pageSize <= 0)
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");

            // skip take logic
            var taskSummaryIds = db.TaskSummary
                .Where(x => x.TaskId == taskId)
                .Select(x => x.TaskSummaryId)
                .OrderByDescending(x => Convert.ToInt32(x))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (taskSummaryIds.Count == 0)
            {
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
            }

            return taskSummaryIds;

        }
    }


    public List<TaskSummaryEditModel> GetFilteredTaskSummaryById(List<string> taskSummaryIds, DateTime? fromDate = null, DateTime? toDate = null)
    {
        List<TaskSummaryEditModel> taskSummaryList = new List<TaskSummaryEditModel>();

        taskSummaryIds.ForEach(x =>
        {
            if (fromDate == null && toDate == null)
            {
                taskSummaryList.Add(_taskSummaryRepository.GetTaskSummaryById(x));
            }
            else if (fromDate == null)
            {
                TaskSummaryEditModel taskSummaryEditModel = _taskSummaryRepository.GetTaskSummaryById(x);
                if (taskSummaryEditModel.Date <= toDate)
                {
                    taskSummaryList.Add(taskSummaryEditModel);
                }
            }

            else if (toDate == null)
            {
                TaskSummaryEditModel taskSummaryEditModel = _taskSummaryRepository.GetTaskSummaryById(x);
                if (taskSummaryEditModel.Date >= fromDate)
                {
                    taskSummaryList.Add(taskSummaryEditModel);
                }
            }

            else
            {
                TaskSummaryEditModel taskSummaryEditModel = _taskSummaryRepository.GetTaskSummaryById(x);
                if (taskSummaryEditModel.Date >= fromDate && taskSummaryEditModel.Date <= toDate)
                {
                    taskSummaryList.Add(taskSummaryEditModel);
                }
            }
        });

        return taskSummaryList;
    }

    private List<string> GetTaskSummaryIdsPageForAProfileId(string profileId, DateTime date, int pageIndex,
        int pageSize)
    {
        using (var db = new ErpContext())
        {
            if (pageIndex <= 0 || pageSize <= 0)
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");

            var taskSummaryIds = db.TaskSummary
                .Where(x => x.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                .Include(x => x.TaskId)
                .Where(x => x.TaskDetail.AssignedTo == profileId)
                .Select(x => x.TaskSummaryId)
                .OrderByDescending(x => Convert.ToInt32(x))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (taskSummaryIds.Count == 0)
            {
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
            }

            return taskSummaryIds;
        }
    }


    public TaskSummaryEditModel AddOrUpdateTaskSummary(TaskSummaryEditModel taskSummaryEditModel)
    {
        return AddOrUpdateTaskSummaryInDb(taskSummaryEditModel);

    }

    private TaskSummaryEditModel AddOrUpdateTaskSummaryInDb(TaskSummaryEditModel taskSummaryEditModel)
    {
        TaskSummary taskSummary;

        using (var db = new ErpContext())
        {
            taskSummary = db.TaskSummary
                .FirstOrDefault(x => x.TaskSummaryId == taskSummaryEditModel.TaskSummaryId);


            if (taskSummary != null) // update
            {
                taskSummary.TaskId = taskSummaryEditModel.TaskId;
                taskSummary.Date = taskSummaryEditModel.Date;
                taskSummary.ExpectedHour = taskSummaryEditModel.ExpectedHour;
                taskSummary.ExpectedOutput = taskSummaryEditModel.ExpectedOutput;
                taskSummary.ActualHour = taskSummaryEditModel.ActualHour;
                taskSummary.ActualOutput = taskSummaryEditModel.ActualOutput;
                taskSummary.TaskScheduleId = taskSummaryEditModel.TaskScheduleId;
                db.SaveChanges();
            }

            else
            {
                taskSummary = new TaskSummary()
                {
                    TaskSummaryId = GetNextAvailableId(),
                    TaskId = taskSummaryEditModel.TaskId,
                    Date = taskSummaryEditModel.Date,
                    ExpectedHour = taskSummaryEditModel.ExpectedHour,
                    ExpectedOutput = taskSummaryEditModel.ExpectedOutput,
                    ActualHour = taskSummaryEditModel.ActualHour,
                    ActualOutput = taskSummaryEditModel.ActualOutput,
                    TaskScheduleId = taskSummaryEditModel.TaskScheduleId
                };
                db.TaskSummary.Add(taskSummary);
                db.SaveChanges();
            }
        }

        // [Action]: Updated edited time of Task Module
        _taskRepository.UpdateEditedAtTimeStamp(taskSummary.TaskId);
        return _taskSummaryRepository.GetTaskSummaryById(taskSummary.TaskSummaryId);
    }

    private static string GetNextAvailableId()
    {
        using (var db = new ErpContext())
        {
            var a = db.TaskSummary
                .Select(x => Convert.ToInt32(x.TaskSummaryId))
                .DefaultIfEmpty(0)
                .Max();
            return Convert.ToString(a + 1);
        }

    }

    public void DeleteTaskSummary(string taskSummaryId)
    {
        using (var db = new ErpContext())
        {
            // Get Selected Profile
            TaskSummary existingTaskSummary = db.TaskSummary
                .FirstOrDefault(x => x.TaskSummaryId == taskSummaryId);

            if (existingTaskSummary != null)
            {

                db.TaskSummary.Remove(existingTaskSummary);
                db.SaveChanges();
            }


        }
    }

    public List<TaskSummaryEditModel> UpdateDailyTaskActualTime(string profileId, string taskSummaryId,
        DateTime stamp, string action, int? pageIndex = null, int? pageSize = null)
    {
        if (_taskSummaryRepository.GetTaskSummaryById(taskSummaryId) == null)
        {
            throw new KeyNotFoundException("Error in finding taskSummaryId");
        }

        List<TaskSummaryEditModel> taskSummaryList
            = new List<TaskSummaryEditModel>();

        if (action == "start")
        {
            List<string> taskSummaryIds;
            string check_start;

            using (var db = new ErpContext())
            {
                check_start = db.TaskSummary
                    .Where(x => x.TaskSummaryId == taskSummaryId)
                    .Select(x => x.Action)
                    .First();


                var usedProfileId = db.TaskSummary
                    .Include(x => x.TaskDetail)
                    .Where(x => x.TaskSummaryId == taskSummaryId)
                    .Select(x => x.TaskDetail.AssignedTo)
                    .First();


                taskSummaryIds = db.TaskSummary
                    .Where(x => x.Action == "start")
                    .Include(x => x.TaskId)
                    .Where(x => x.TaskDetail.AssignedTo == usedProfileId)
                    .Select(x => x.TaskSummaryId)
                    .ToList();
            }

            if (check_start == "start")
            {
                throw new ArgumentException("Cannot start task which is already start.");
            }

            taskSummaryIds.ForEach(x =>
            {
                taskSummaryList.Add(UpdateTaskSummaryActionAndActualTimeInDb(x, stamp, "stop"));
            });

            taskSummaryList.Add(UpdateTaskSummaryActionAndActualTimeInDb(taskSummaryId, stamp, action));

            if (pageIndex != null && pageSize != null)
            {
                return GetTaskSummaryListPage(taskSummaryList, (int) pageIndex, (int) pageSize);
            }

            return taskSummaryList;

        }

        string check_action;

        using (var db = new ErpContext())
        {
            check_action = db.TaskSummary
                .Where(x => x.TaskSummaryId == taskSummaryId)
                .Select(x => x.Action)
                .First();
        }

        if (check_action == "stop")
        {
            throw new ArgumentException("Cannot stop task which is already stop.");
        }

        taskSummaryList.Add(UpdateTaskSummaryActionAndActualTimeInDb(taskSummaryId, stamp, action));

        if (pageIndex != null && pageSize != null)
        {
            return GetTaskSummaryListPage(taskSummaryList, (int) pageIndex, (int) pageSize);
        }

        return taskSummaryList;
    }

    private static List<TaskSummaryEditModel> GetTaskSummaryListPage(List<TaskSummaryEditModel> taskSummaryList,
        int pageIndex, int pageSize)
    {
        if (pageIndex <= 0 || pageSize <= 0)
            throw new ArgumentException("Incorrect value for pageIndex or pageSize");

        // skip take logic
        var taskSummaryPage = taskSummaryList
            .OrderByDescending(x => Convert.ToInt32(x.TaskSummaryId))
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (taskSummaryPage.Count == 0)
        {
            throw new ArgumentException("Incorrect value for pageIndex or pageSize");
        }

        return taskSummaryPage;
    }

    private TaskSummaryEditModel UpdateTaskSummaryActionAndActualTimeInDb(string taskSummaryId,
        DateTime stamp, string action)
    {
        TaskSummary taskSummaryDb;
        TaskSummaryEditModel taskSummary = new TaskSummaryEditModel();

        using (var db = new ErpContext())
        {
            taskSummaryDb = db.TaskSummary
                .FirstOrDefault(x => x.TaskSummaryId == taskSummaryId);

            if (action == "stop")
            {
                decimal temp = new decimal();
                temp = taskSummaryDb.SystemHours;
                taskSummaryDb.SystemHours = temp + (decimal) (stamp - taskSummaryDb.Stamp).TotalMinutes / 60;
            }

            taskSummaryDb.Stamp = stamp;
            taskSummaryDb.Action = action;
            db.SaveChanges();
        }

        taskSummary = _taskSummaryRepository.GetTaskSummaryById(taskSummaryId);
        return taskSummary;

    }


    public decimal GetTotalEstimatedHoursForTask(string taskId)
    {
        return GetAllTaskSummaryByTaskId(taskId, null, null, "allChildren")
            .Sum(x => x.ActualHour);
    }

    public decimal GetTotalActualHoursForTask(string taskId)
    {
        return GetAllTaskSummaryByTaskId(taskId, null, null, "allChildren")
            .Sum(x => x.ExpectedHour);
    }
    }
}