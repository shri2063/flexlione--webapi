using System.Collections.Generic;
using System.Threading.Tasks;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
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
        
        public List<TaskSheetEditModel> GetTaskList([FromQuery] string include,string taskId = null)
        {
            return TaskManagerService.GetTaskList(taskId,include);
        }
        
        [HttpPut("CreateOrUpdateTask")]
        [Consumes("application/json")]
        
        public TaskSheetEditModel CreateOrUpdateTask(TaskSheetEditModel task)
        {
            return TaskManagerService.CreateOrUpdateTask(task);
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