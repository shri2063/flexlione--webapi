using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.LinkedListModel;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Mvc;
using flexli_erp_webapi.Models;
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
        
        [HttpGet("GetTaskById")]
        [Consumes("application/json")]
        
        public ActionResult<TaskDetailEditModel> GetTaskById([FromQuery] string taskId,string include = null)
        {
            return TaskManagementService.GetTaskById(taskId,include);
        }
        
        [HttpGet("GetTaskIdList")]
        [Consumes("application/json")]
        
        public List<TaskShortDetailEditModel> GetTaskIdList(string taskId = null)
        {
            return TaskManagementService.GetTaskIdList(taskId);
        }
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new taskDetail - what  Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        
       
        public ActionResult<TaskDetailEditModel> CreateOrUpdateTask(TaskDetailEditModel taskDetail)
        {
            return TaskManagementService.CreateOrUpdateTask(taskDetail);
        }
        /// <summary>
        /// Create or update tasks
        /// [Check] Task not linked to another sprint
        /// </summary>
        [HttpPut("LinkTaskToSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId)
        {
            return TaskManagementService.LinkTaskToSprint(taskId, sprintId);
        }
        
        
        [HttpDelete("DeleteTask")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTask(string taskId)
        {
            TaskManagementService.DeleteTask(taskId);
            return Ok();
        }
        
        // Removed task will not be shown in Web App until forced
        [HttpPost("RemoveTask")]
        [Consumes("application/json")]
        
        public ActionResult<string> RemoveTask(string taskId)
        {
            TaskManagementService.RemoveTask(taskId);
            return Ok();
        }

        [HttpPut("RemoveTaskFromSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel RemoveTaskToSprint(string taskId)
        {
            return TaskManagementService.RemoveTaskFromSprint(taskId);
        }

        [HttpPost("UpdateActualScoreOfTask")]
        [Consumes("application/json")]

        public TaskDetailEditModel UpdateActualScoreOfTask(string taskId, string include, int actualScore)
        {
            // return TaskManagementService.UpdateActualScoreOfTask(taskId, include, actualScore);
            return null;
        }
    }
}