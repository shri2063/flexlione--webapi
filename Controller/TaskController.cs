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
        
        public TaskEditModel GetTaskById([FromQuery] string taskId,string include = null)
        {
            return TaskManagerService.GetTaskById(taskId,include);
        }
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new task - what task Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        
        public ActionResult<TaskEditModel> CreateOrUpdateTask(TaskEditModel task)
        {
            return TaskManagerService.CreateOrUpdateTask(task);
        }
        
        [HttpPut("GetLinkedList")]
        [Consumes("application/json")]
        
        public LinkedChildTaskHead GetLinkedList(string taskId = null)
        {
            List<TaskEditModel> taskList = TaskManagerService.GetTaskById(taskId, "children").Children;
            return LinkedListService.CreateLinkedList(taskList);
        }

        
        [HttpDelete("DeleteTask")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteTask(string taskId)
        {
            TaskManagerService.DeleteTask(taskId);
            return Ok();
        }
    }
}