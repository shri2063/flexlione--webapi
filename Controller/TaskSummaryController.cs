using System;
using System.Collections.Generic;
using m_sort_server.EditModels;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace m_sort_server.Controller
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

        [HttpGet("GetAllTaskSummaryByTaskId")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> GetAllTaskSummaryByTaskId(string taskId, string include = null)
        {
            return TaskSummaryManagementService.GetAllTaskSummaryByTaskId(taskId, include);
        }

        [HttpPost("AddOrUpdateTaskSummary")]
        [Consumes("application/json")]
        public TaskSummaryEditModel AddOrUpdateTaskSummary(TaskSummaryEditModel taskSummaryEditModel)
        {
            return TaskSummaryManagementService.AddOrUpdateTaskSummary(taskSummaryEditModel);
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