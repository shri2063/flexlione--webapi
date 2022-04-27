using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Services
{
    public class TaskScheduleManagementService
    {
        public static TaskScheduleEditModel GetTaskScheduleById(string taskScheduleId, string include = null)
        {
            return GetTaskScheduleByIdFromDb(taskScheduleId);

        }


        public static List<TaskScheduleEditModel> GetAllTaskScheduleByProfileId(string profileId, int month,int year, string include = null)
        {
            List<string> taskScheduleIds = GetTaskScheduleIdsForProfileId(profileId,month,year);
            List<TaskScheduleEditModel> taskScheduleList = new List<TaskScheduleEditModel>();
            taskScheduleIds.ForEach(x =>
            {
                taskScheduleList.Add(GetTaskScheduleById(x));
            });
            return taskScheduleList;

        }

       
        
        private static List<string> GetTaskScheduleIdsForProfileId(string profileId, int month, int year)
        {
            List<string> taskScheduleIds;
            using (var db = new ErpContext())
            {
                taskScheduleIds = db.TaskSchedule
                    .Where(x => x.Owner == profileId && 
                                x.Date.Month == month && x.Date.Year == year)
                    .Select(x => x.TaskScheduleId)
                    .ToList();
            }

            return taskScheduleIds;

        }
        
        private static TaskScheduleEditModel GetTaskScheduleByIdFromDb (string taskScheduleId)
        {
            using (var db = new ErpContext())
            {
                
                TaskSchedule existingTaskSchedule = db.TaskSchedule
                    .Include(x => x.TaskDetail)
                    .Include(x => x.TaskSummary)
                    .FirstOrDefault(x => x.TaskScheduleId == taskScheduleId);
                
                // Case: Task Schedule does not exist
                if (existingTaskSchedule == null)
                {
                    return null;
                }
                    
                
                // Case: In case you have to update data received from db

                TaskScheduleEditModel taskScheduleEditModel = new TaskScheduleEditModel()
                {
                    TaskScheduleId = existingTaskSchedule.TaskScheduleId,
                    Description = existingTaskSchedule.TaskDetail.Description,
                    Owner = existingTaskSchedule.Owner,
                    TaskId = existingTaskSchedule.TaskId,
                    Date = existingTaskSchedule.Date,
                    StartHour = existingTaskSchedule.StartHour,
                    StopHour = existingTaskSchedule.StopHour,
                    StartMinute = existingTaskSchedule.StartMinute,
                    StopMinute = existingTaskSchedule.StopMinute,
                    TaskSummaryId = existingTaskSchedule.TaskSummary == null ? "" :existingTaskSchedule.TaskSummary.TaskSummaryId
                };

                return taskScheduleEditModel;
            }
        }
        
        public static TaskScheduleEditModel AddOrUpdateTaskSchedule(TaskScheduleEditModel taskScheduleEditModel)
        {
            return AddOrUpdateTaskSummaryInDb(taskScheduleEditModel);

        }
        
        private static TaskScheduleEditModel AddOrUpdateTaskSummaryInDb(TaskScheduleEditModel taskScheduleEditModel)
        {
            TaskSchedule taskSchedule;
            
            using (var db = new ErpContext())
            {
                taskSchedule = db.TaskSchedule
                    .FirstOrDefault(x => x.TaskScheduleId == taskScheduleEditModel.TaskScheduleId);


                if (taskSchedule != null) // update
                {
                    taskSchedule.Owner = taskScheduleEditModel.Owner;
                    taskSchedule.TaskId = taskScheduleEditModel.TaskId;
                    taskSchedule.Date = taskScheduleEditModel.Date;
                    taskSchedule.StartHour = taskScheduleEditModel.StartHour;
                    taskSchedule.StopHour = taskScheduleEditModel.StopHour;
                    taskSchedule.StartMinute = taskScheduleEditModel.StartMinute;
                    taskSchedule.StopMinute = taskScheduleEditModel.StopMinute;
                    db.SaveChanges();
                }
                else
                {
                    taskSchedule = new TaskSchedule()
                    {
                        TaskScheduleId = GetNextAvailableId(),
                        Owner = taskScheduleEditModel.Owner,
                        TaskId = taskScheduleEditModel.TaskId,
                        Date = taskScheduleEditModel.Date,
                        StartHour = taskScheduleEditModel.StartHour,
                        StopHour = taskScheduleEditModel.StopHour,
                        StartMinute = taskScheduleEditModel.StartMinute,
                        StopMinute = taskScheduleEditModel.StopMinute
                    };
                    db.TaskSchedule.Add(taskSchedule);
                    db.SaveChanges();
                }
            }

            return GetTaskScheduleById(taskSchedule.TaskScheduleId);
        }
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.TaskSchedule
                    .Select(x => Convert.ToInt32(x.TaskScheduleId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public static void DeleteTaskSchedule(string taskScheduleId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                TaskSchedule existingTaskSchedule = db.TaskSchedule
                    .FirstOrDefault(x => x.TaskScheduleId == taskScheduleId);
                
                if (existingTaskSchedule == null)
                {
                    throw new Exception("Task Schedule does not exist");
                }

                if (existingTaskSchedule.Date < DateTime.Today)
                {
                    throw new Exception("Cannot delete schedule of previous days");
                }
                db.TaskSchedule.Remove(existingTaskSchedule);
                db.SaveChanges();


            }
        }
    }
}