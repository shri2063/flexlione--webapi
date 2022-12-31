using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.LinkedListModel;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Mvc;
using flexli_erp_webapi.Models;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Cors;


namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class TaskController : ControllerBase
    {
       
        private readonly TaskManagementService _taskManagementService;
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskRelationRepository _taskRelationRepository;
        private readonly TaskHierarchyManagementService _taskHierarchyManagementService;
        
        
       
        public TaskController(TaskManagementService taskManagementService, ITaskRepository taskRepository, 
            ITaskRelationRepository taskRelationRepository, TaskHierarchyManagementService taskHierarchyManagementService)
        {
           
            _taskManagementService = taskManagementService;
            _taskRepository = taskRepository;
            _taskRelationRepository = taskRelationRepository;
            _taskHierarchyManagementService = taskHierarchyManagementService;
        }

       
      
        /// <summary>
        /// Include: children, siblings, dependency
        /// </summary>
        [HttpGet("GetTaskById")]
        [Consumes("application/json")]

        public ActionResult<TaskDetailEditModel> GetTaskById([FromQuery] string taskId,string include = null)
        {
            return _taskManagementService.GetTaskById(taskId,include);
        }
        
        [HttpGet("GetTaskIdList")]
        [Consumes("application/json")]
        
        public List<TaskShortDetailEditModel> GetTaskIdList(string taskId = null, int? pageIndex = null, int? pageSize = null)
        {
            return _taskRepository.GetTaskIdList(taskId, pageIndex, pageSize);
        }
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new taskDetail - what  Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        

        public async Task<ActionResult<TaskDetailEditModel>> CreateOrUpdateTask(TaskDetailEditModel taskDetail)

        {
             return _taskManagementService.CreateOrUpdateTask(taskDetail);
        
        }
        /// <summary>
        /// Create or update tasks
        /// [Check] Task not linked to another sprint
        /// </summary>
        [HttpPut("LinkTaskToSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId)
        {
            return _taskRelationRepository.LinkTaskToSprint(taskId, sprintId);
        }
        
        
        [HttpDelete("DeleteTask")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTask(string taskId)
        {
            _taskManagementService.DeleteTask(taskId);
            return Ok();
        }
        
        // Removed task will not be shown in Web App until forced
        [HttpPost("RemoveTask")]
        [Consumes("application/json")]
        
        public ActionResult<string> RemoveTask(string taskId)
        {
            _taskManagementService.RemoveTask(taskId);
            return Ok();
        }

        [HttpPut("RemoveTaskFromSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel RemoveTaskToSprint(string taskId)
        {
            return _taskRelationRepository.RemoveTaskFromSprint(taskId);
        }
        
        /// <summary>
        /// Enter taskId to label it, currently label = "sprint" only
        /// </summary>
        [HttpPut("AddLabelToTask")]
        [Consumes("application/json")]

        public async Task<SprintLabelTask> AddLabelToTask(string taskId, string label)
        {
            return await _taskManagementService.AddLabelToTask(taskId, label);
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

       
    }
}