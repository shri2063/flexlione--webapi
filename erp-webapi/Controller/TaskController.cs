using System.Collections.Generic;
using System.Threading.Tasks;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;
using m_sort_server.Services;
using Microsoft.AspNetCore.Mvc;
using m_sort_server.Models;
using Microsoft.AspNetCore.Cors;


namespace m_sort_server.Controller
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
        /// Note that if you are creating new taskDetail - what taskDetail Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        
        public ActionResult<TaskDetailEditModel> CreateOrUpdateTask(TaskDetailEditModel taskDetail)
        {
            return TaskManagementService.CreateOrUpdateTask(taskDetail);
        }
        
        [HttpPut("GetLinkedList")]
        [Consumes("application/json")]
        
        public LinkedChildTaskHead GetLinkedList(string taskId = null)
        {
            List<TaskDetailEditModel> taskList = TaskManagementService.GetTaskById(taskId, "children").Children;
            return LinkedListService.CreateLinkedList(taskList);
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
        
        [HttpPut("LinkTaskToSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId)
        {
            return TaskManagementService.LinkTaskToSprint(taskId, sprintId);
        }
        
        [HttpPut("RemoveTaskFromSprint")]
        [Consumes("application/json")]
        
        public TaskDetailEditModel RemoveTaskToSprint(string taskId)
        {
            return TaskManagementService.RemoveTaskFromSprint(taskId);
        }
    }
}