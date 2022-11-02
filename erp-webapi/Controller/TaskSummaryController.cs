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
    
    public class TaskSummaryController : ControllerBase
    {
        [HttpGet("GetTaskSummaryById")]
        [Consumes("application/json")]

        public TaskSummaryEditModel GetTaskSummaryById(string taskSummaryId)
        {
            return TaskSummaryManagementService.GetTaskSummaryById(taskSummaryId);
        }
        
        [HttpGet("GetDailyTaskSummary")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> GetDailyTaskSummary(string profileId, DateTime date, int? pageIndex = null, int? pageSize = null)
        {
            return TaskSummaryManagementService.GetDailyTaskSummary(profileId, date, pageIndex, pageSize);
        }

        

        [HttpGet("GetAllTaskSummaryByTaskId")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> GetAllTaskSummaryByTaskId(string taskId, DateTime? fromDate, DateTime? toDate=null, string include=null, int? pageIndex = null, int? pageSize = null)
        {
            return TaskSummaryManagementService.GetAllTaskSummaryByTaskId(taskId, fromDate, toDate, include, pageIndex, pageSize);
        }

        [HttpPost("AddOrUpdateTaskSummary")]
        [Consumes("application/json")]
        public TaskSummaryEditModel AddOrUpdateTaskSummary(TaskSummaryEditModel taskSummaryEditModel)
        {
            return TaskSummaryManagementService.AddOrUpdateTaskSummary(taskSummaryEditModel);
        }
        
        //created by Tushar Garg
        [HttpPost("UpdateDailyTaskActualTime")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> UpdateDailyTaskActualTime(string profileId, string taskSummaryId, DateTime stamp, string action, int? pageIndex = null, int? pageSize = null)
        {
            return TaskSummaryManagementService.UpdateDailyTaskActualTime(profileId, taskSummaryId, stamp, action, pageIndex, pageSize);
        }
        
        [HttpDelete("DeleteTaskSummary")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTaskSummary(string taskSummaryId)
        {
            TaskSummaryManagementService.DeleteTaskSummary(taskSummaryId);
            return Ok();
        } 
    }
    
}