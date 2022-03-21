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
        
        public List<TaskSheetItemEditModel> GetTaskList([FromQuery] string include,string taskId = null)
        {
            return TaskManagerService.GetTaskList(taskId,include);
        }
        
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        
        public TaskSheetItemEditModel CreateOrUpdateTask(TaskSheetItemEditModel task)
        {
            return TaskManagerService.CreateOrUpdateTask(task);
        }
        
        [HttpPut("GetLinkedList")]
        [Consumes("application/json")]
        
        public LinkedChildTaskHead GetLinkedList(string taskId = null)
        {
            List<TaskSheetItemEditModel> taskList = TaskManagerService.GetTaskList(taskId, "children");
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