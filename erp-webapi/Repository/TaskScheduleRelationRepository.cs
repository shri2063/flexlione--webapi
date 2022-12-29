using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class TaskScheduleRelationRepository: ITaskScheduleRelationRepository
    {

        private readonly ITaskSummaryRepository _taskSummaryRepository;
        private readonly ITaskScheduleRepository _taskScheduleRepository;

        public TaskScheduleRelationRepository(ITaskSummaryRepository taskSummaryRepository, ITaskScheduleRepository taskScheduleRepository)
        {
            _taskSummaryRepository = taskSummaryRepository;
            _taskScheduleRepository = taskScheduleRepository;
        }
        public  List<TaskScheduleEditModel> GetAllTaskScheduleByProfileIdAndMonth(string profileId, int month,int year, string include = null, int? pageIndex = null, int? pageSize = null)
        {
            List<string> taskScheduleIds;
            
            taskScheduleIds = GetTaskScheduleIdsForProfileId(profileId,month,year, pageIndex, pageSize);
            
            List<TaskScheduleEditModel> taskScheduleList = new List<TaskScheduleEditModel>();
            taskScheduleIds.ForEach(x =>
            {
                taskScheduleList.Add(_taskScheduleRepository.GetTaskScheduleByIdFromDb(x));
            });
            
            taskScheduleList.ForEach(x =>
            {
                
                {
                    TaskSummaryEditModel taskSummary =  _taskSummaryRepository.GetTaskSummaryById(x.TaskSummaryId);
                    if (taskSummary != null)
                    {
                        x.TaskSummary = new TaskShortSummaryEditModel()
                        {
                            TaskSummaryId = taskSummary.TaskSummaryId,
                            TaskId = taskSummary.TaskId,
                            ActualOutput = taskSummary.ActualOutput
                        }; 
                    }
                }
            });
            
            return taskScheduleList;

        }
        
        public  List<TaskScheduleEditModel> GetAllTaskScheduleByProfileIdAndDateRange(string profileId,DateTime fromDate, DateTime toDate )
        {
            List<TaskScheduleEditModel> taskScheduleList = new List<TaskScheduleEditModel>();
            using (var db = new ErpContext())
            {
                List<TaskSchedule> taskSchedules = db.TaskSchedule
                    .Where(x => x.Owner == profileId && 
                                x.Date >= fromDate && x.Date <= toDate)
                    .ToList();
               
                taskSchedules.ForEach(x => taskScheduleList
                    .Add(_taskScheduleRepository.GetTaskScheduleByIdFromDb(x.TaskScheduleId)));
            }

            return taskScheduleList;
        }




        
        private  List<string> GetTaskScheduleIdsForProfileId(string profileId, int month, int year, int? pageIndex = null, int? pageSize = null)
        {
            List<string> taskScheduleIds;
            using (var db = new ErpContext())
            {
                // [Check] : Pagination
                if (pageIndex != null && pageSize != null)
                {
                    if (pageIndex <= 0 || pageSize <= 0)
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                    // skip take logic
                    taskScheduleIds = db.TaskSchedule
                        .Where(x => x.Owner == profileId && 
                                    x.Date.Month == month && x.Date.Year == year)
                        .Select(x => x.TaskScheduleId).AsEnumerable()
                        .OrderByDescending(Convert.ToInt32)
                        .Skip(((int) pageIndex - 1) * (int) pageSize)
                        .Take((int) pageSize)
                        .ToList();

                    if (taskScheduleIds.Count == 0)
                    {
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                    }

                    return taskScheduleIds;
                }
                
                taskScheduleIds = db.TaskSchedule
                    .Where(x => x.Owner == profileId && 
                                x.Date.Month == month && x.Date.Year == year)
                    .Select(x => x.TaskScheduleId)
                    .ToList();
            }

            return taskScheduleIds;

        }

    }
}