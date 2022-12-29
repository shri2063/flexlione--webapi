using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using m_sort_server.Repository.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    
    public class TaskHierarchyController:ControllerBase
    
    {
        private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private readonly TaskHierarchyManagementService _taskHierarchyManagementService;
        
        
       
        public TaskHierarchyController(ITaskHierarchyRelationRepository taskHierarchyRelationRepository, TaskHierarchyManagementService taskHierarchyManagementService){
            
           
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
            _taskHierarchyManagementService = taskHierarchyManagementService;


        }
        
        /// <summary>
        /// Include: children (to get task hierarchies of each children)
        /// </summary>
        [HttpGet("GetTaskHierarchyByTaskId")]
        [Consumes("application/json")]

        public ActionResult<TaskHierarchyEditModel> GetTaskHierarchyByTaskId(string taskId,string include = null)
        {
            return _taskHierarchyManagementService.GetTaskHierarchyByTaskId(taskId,include);
        }
        
        // Updates Upstream hierarchy till Head Task for all tasks
        // downstream of the given task (including task)
        // If null is selected, it will update upstream hierarchy of all tasks in db
        [HttpPut("UpdateTaskHierarchy")]
        [Consumes("application/json")]

        public ActionResult<List<TaskHierarchyEditModel>> UpdateTaskHierarchy(string taskId =  null)
        {
            return _taskHierarchyRelationRepository.UpdateTaskHierarchy(taskId);
        }
        
        [HttpDelete("DeleteTaskHierarchy")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTaskHierarchy(string taskId)
        {
            _taskHierarchyRelationRepository.DeleteTaskHierarchy(taskId);
            return Ok();
        } 
        
       
    }
}
