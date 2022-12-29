using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
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
        
        private readonly ITaskSummaryRepository _taskSummaryRepository;
        private readonly TaskSummaryManagementService _taskSummaryManagementService;



        public TaskSummaryController(ITaskSummaryRepository taskSummaryRepository, 
            TaskSummaryManagementService taskSummaryManagementService){
            
           
            _taskSummaryRepository = taskSummaryRepository;
            _taskSummaryManagementService = taskSummaryManagementService;
          

        }
        [HttpGet("GetTaskSummaryById")]
        [Consumes("application/json")]

        public TaskSummaryEditModel GetTaskSummaryById(string taskSummaryId)
        {
            return _taskSummaryRepository.GetTaskSummaryById(taskSummaryId);
        }
        
        [HttpGet("GetDailyTaskSummary")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> GetDailyTaskSummary(string profileId, DateTime date, int? pageIndex = null, int? pageSize = null)
        {
            return _taskSummaryManagementService.GetDailyTaskSummary(profileId, date, pageIndex, pageSize);
        }

        /// <summary>
        /// include: allChildren
        /// </summary>

        [HttpGet("GetAllTaskSummaryByTaskId")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> GetAllTaskSummaryByTaskId(string taskId, DateTime? fromDate, DateTime? toDate=null, string include=null, int? pageIndex = null, int? pageSize = null)
        {
            return _taskSummaryManagementService.GetAllTaskSummaryByTaskId(taskId, fromDate, toDate, include, pageIndex, pageSize);
        }

        [HttpPost("AddOrUpdateTaskSummary")]
        [Consumes("application/json")]
        public TaskSummaryEditModel AddOrUpdateTaskSummary(TaskSummaryEditModel taskSummaryEditModel)
        {
            return _taskSummaryManagementService.AddOrUpdateTaskSummary(taskSummaryEditModel);
        }
        
        /// <summary>
        /// action: start,stop
        /// </summary>
        [HttpPost("UpdateDailyTaskActualTime")]
        [Consumes("application/json")]

        public List<TaskSummaryEditModel> UpdateDailyTaskActualTime(string profileId, string taskSummaryId, DateTime stamp, string action, int? pageIndex = null, int? pageSize = null)
        {
            return _taskSummaryManagementService.UpdateDailyTaskActualTime(profileId, taskSummaryId, stamp, action, pageIndex, pageSize);
        }
        
        [HttpDelete("DeleteTaskSummary")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTaskSummary(string taskSummaryId)
        {
            _taskSummaryManagementService.DeleteTaskSummary(taskSummaryId);
            return Ok();
        } 
    }
    
}