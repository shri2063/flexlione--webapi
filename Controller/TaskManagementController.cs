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
    [Route("api/v1")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class TaskManagerController : ControllerBase
    {
        
        [HttpGet("GetTaskList")]
        [Consumes("application/json")]
        
        public List<TaskEditModel> GetTaskList([FromQuery] string include,string taskId = null)
        {
            return TaskManagerService.GetTaskList(taskId,include);
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
            List<TaskEditModel> taskList = TaskManagerService.GetTaskList(taskId, "children");
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