using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    
    public class TaskScheduleController:ControllerBase
    
    {
        [HttpGet("GetTaskScheduleById")]
        [Consumes("application/json")]

        public TaskScheduleEditModel GetTaskScheduleById(string taskScheduleId, string include)
        {
            return TaskScheduleManagementService.GetTaskScheduleById(taskScheduleId, include);
        }
        
        [HttpGet("GetTaskScheduleForProfileId")]
        [Consumes("application/json")]

        public List<TaskScheduleEditModel> GetTaskScheduleForProfileId(string profileId, int month, int year)
        {
            return TaskScheduleManagementService.GetAllTaskScheduleByProfileIdAndMonth(profileId, month, year);
        }
        
       
        
       
        
       
        [HttpPost("AddOrUpdateTaskSchedule")]
        [Consumes("application/json")]
        public TaskScheduleEditModel AddOrUpdateTaskSchedule(TaskScheduleEditModel taskScheduleEditModel)
        {
            return TaskScheduleManagementService.AddOrUpdateTaskSchedule(taskScheduleEditModel);
        }
        
        [HttpDelete("DeleteTaskSchedule")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTaskSchedule(string taskScheduleId)
        {
            TaskScheduleManagementService.DeleteTaskSchedule(taskScheduleId);
            return Ok();
        } 
    }
}
