using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
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
    
    
    public class TaskScheduleController:ControllerBase
    
    {
        private readonly ITaskScheduleRepository _taskScheduleRepository;
        private readonly ITaskScheduleRelationRepository _taskScheduleRelationRepository;
        private readonly TaskScheduleManagementService _taskScheduleManagementService;
        
        
       
        public TaskScheduleController(ITaskScheduleRepository taskScheduleRepository, ITaskScheduleRelationRepository taskScheduleRelationRepository,
            TaskScheduleManagementService taskScheduleManagementService){
            
           
            _taskScheduleRepository = taskScheduleRepository;
            _taskScheduleManagementService = taskScheduleManagementService;
            _taskScheduleRelationRepository = taskScheduleRelationRepository;


        }
        
        /// <summary>
        /// include: taskSummary
        /// </summary>
        [HttpGet("GetTaskScheduleById")]
        [Consumes("application/json")]

        public TaskScheduleEditModel GetTaskScheduleById(string taskScheduleId, string include)
        {
            return _taskScheduleManagementService.GetTaskScheduleById(taskScheduleId, include);
        }
        
        /// <summary>
        /// include: "Nothing is available in include"
        /// </summary>
        [HttpGet("GetTaskScheduleForProfileId")]
        [Consumes("application/json")]

        public List<TaskScheduleEditModel> GetTaskScheduleForProfileId(string profileId, int month, int year, string include = null,  int? pageIndex = null, int? pageSize = null)
        {
            return _taskScheduleRelationRepository.GetAllTaskScheduleByProfileIdAndMonth(profileId, month, year, include, pageIndex, pageSize);
        }
        
       
        
       
        
       
        [HttpPost("AddOrUpdateTaskSchedule")]
        [Consumes("application/json")]
        public TaskScheduleEditModel AddOrUpdateTaskSchedule(TaskScheduleEditModel taskScheduleEditModel)
        {
            return _taskScheduleManagementService.AddOrUpdateTaskSchedule(taskScheduleEditModel);
        }
        
        [HttpDelete("DeleteTaskSchedule")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTaskSchedule(string taskScheduleId)
        {
            _taskScheduleManagementService.DeleteTaskSchedule(taskScheduleId);
            return Ok();
        } 
    }
}
