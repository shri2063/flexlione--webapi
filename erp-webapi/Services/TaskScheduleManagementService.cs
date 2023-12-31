﻿using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Services
{
    public class TaskScheduleManagementService
    {

        private readonly ITaskSummaryRepository _taskSummaryRepository;
        private readonly ITaskScheduleRepository _taskScheduleRepository;
        private readonly ITaskRepository _taskRepository;
        public TaskScheduleManagementService(ITaskSummaryRepository taskSummaryRepository, ITaskScheduleRepository taskScheduleRepository,
            ITaskRepository taskRepository)
        {
            _taskSummaryRepository = taskSummaryRepository;
            _taskScheduleRepository = taskScheduleRepository;
            _taskRepository = taskRepository;
        }
        public  TaskScheduleEditModel GetTaskScheduleById(string taskScheduleId, string include = null)
        {
            TaskScheduleEditModel taskScheduleEditModel = _taskScheduleRepository.GetTaskScheduleByIdFromDb(taskScheduleId);
            if (include == null)
            {
                return taskScheduleEditModel;
            }

            if (include.Contains("taskSummary"))
            {
                TaskSummaryEditModel taskSummary = _taskSummaryRepository.GetTaskSummaryById(
                    taskScheduleEditModel.TaskSummaryId);
                if (taskSummary != null)
                {
                    taskScheduleEditModel.TaskSummary = new TaskShortSummaryEditModel()
                    {
                        TaskSummaryId = taskSummary.TaskSummaryId,
                        TaskId = taskSummary.TaskId,
                        ActualOutput = taskSummary.ActualOutput
                    };
                }
               
                return taskScheduleEditModel;
            }
           
            return taskScheduleEditModel;

        }


       

       
        public  TaskScheduleEditModel AddOrUpdateTaskSchedule(TaskScheduleEditModel taskScheduleEditModel)
        {
            return AddOrUpdateTaskSummaryInDb(taskScheduleEditModel);

        }
        
        private  TaskScheduleEditModel AddOrUpdateTaskSummaryInDb(TaskScheduleEditModel taskScheduleEditModel)
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
                    taskSchedule.IsPlanned = taskScheduleEditModel.IsPlanned;
                    db.SaveChanges();
                }
                else
                {
                    // Can not plan stand up after 10.18,  only unplanned task can be added.
                    if (taskScheduleEditModel.IsPlanned && DateTime.Now > taskScheduleEditModel.Date.AddHours(10).AddMinutes(18))

                    {
                        throw new Exception("Daily Stand up editing locked. Can not add planned task after 10:18 AM. Please schedule as unplanned task");
                    }

                    
                    taskSchedule = new TaskSchedule()
                    {
                        TaskScheduleId = GetNextAvailableId(),
                        Owner = taskScheduleEditModel.Owner,
                        TaskId = taskScheduleEditModel.TaskId,
                        Date = taskScheduleEditModel.Date,
                        StartHour = taskScheduleEditModel.StartHour,
                        StopHour = taskScheduleEditModel.StopHour,
                        StartMinute = taskScheduleEditModel.StartMinute,
                        StopMinute = taskScheduleEditModel.StopMinute,
                        IsPlanned = taskScheduleEditModel.IsPlanned
                    };
                    db.TaskSchedule.Add(taskSchedule);
                    db.SaveChanges();
                    // [Action]: Updated edited time of Task Module
                    _taskRepository.UpdateEditedAtTimeStamp(taskSchedule.TaskId);
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
        
        public  void DeleteTaskSchedule(string taskScheduleId)
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
        
        //created by Tushar Garg
        // function to read taskScheduleId and isPlanned value.
        public static TaskShortScheduleEditModel GetShortTaskScheduleById(string taskScheduleId)
        {
            TaskShortScheduleEditModel taskShortSchedule = GetShortTaskScheduleByIdFromDb(taskScheduleId);

            // exception to handle if no schedule exists
            if (taskShortSchedule == null)
            {
                throw new KeyNotFoundException("Error in finding required taskSchedule");
            }

            return taskShortSchedule;
        }
        
        // reading the values from database
        private static TaskShortScheduleEditModel GetShortTaskScheduleByIdFromDb(string taskScheduleId)
        {
            using (var db = new ErpContext())
            {

                // where taskScheduleId matches parameter Id
                TaskSchedule existingTask = db.TaskSchedule
                    .FirstOrDefault(x => x.TaskScheduleId == taskScheduleId);

                // Case: TaskSchedule does not exist
                if (existingTask == null)
                    return null;
                
                // new object and assign values
                TaskShortScheduleEditModel taskShortScheduleEditModel = new TaskShortScheduleEditModel()
                {
                    TaskScheduleId = existingTask.TaskScheduleId,
                    IsPlanned = existingTask.IsPlanned,
                    StartHour = existingTask.StartHour
                };

                return taskShortScheduleEditModel;

            }
        }
    }
}